using LumiaFoundation.AspNetCore.Commons.Exceptions;
using LumiaFoundation.AspNetCore.ExceptionHandlers.ErrorModel;
using LumiaFoundation.Logger.Contracts;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;

namespace LumiaFoundation.AspNetCore.Commons.Extensions
{
    public static class ExceptionMiddlewareExtensions
    {
        public static void ConfigureExceptionHandler(this WebApplication app, ILoggerManager logger)
        {
            app.UseExceptionHandler(appError =>
            {
                appError.Run(async context =>
                {
                    var exception = context.Features.Get<IExceptionHandlerFeature>()?.Error;

                    if (exception is not null)
                    {
                        context.Response.ContentType = "application/json";
                        await HandleExceptionAsync(context, logger, exception);
                    }
                });
            });
        }

        private static async Task HandleExceptionAsync(HttpContext context, ILoggerManager logger, Exception exception)
        {
            LogException(logger, exception);
            var errorDetails = CreateErrorDetails(exception);
            context.Response.StatusCode = errorDetails.StatusCode;
            await context.Response.WriteAsync(errorDetails.ToString());
        }

        private static ErrorDetails CreateErrorDetails(Exception exception) => exception switch
        {
            DomainBaseException domainException => new ErrorDetails
            {
                StatusCode = domainException.StatusCode,
                Message = domainException.Message,
                ExceptionType = domainException.GetType().Name
            },
            _ => new ErrorDetails
            {
                StatusCode = StatusCodes.Status500InternalServerError,
                Message = "Internal Server Error.",
                ExceptionType = nameof(exception)
            }
        };

        private static void LogException(ILoggerManager logger, Exception exception)
        {
            if (exception is DomainBaseException domainException)
            {
                LogDomainException(logger, domainException);
                return;
            }

            logger.LogError(exception, $"Ocorreu um erro desconhecido: {exception}");
        }

        private static void LogDomainException(ILoggerManager logger, DomainBaseException exception)
        {
            switch (exception.StatusCode)
            {
                case >= 500 and < 600:
                    logger.LogError(exception, $"Ocorreu um erro interno: {exception.Message}");
                    break;
                case >= 400 and < 500:
                    logger.LogWarn("Atenção: {0}", exception);
                    break;
            }
        }
    }
}