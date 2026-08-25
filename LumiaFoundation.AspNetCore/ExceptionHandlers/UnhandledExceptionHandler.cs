using System;
using System.Threading.Tasks;
using LumiaFoundation.AspNetCore.Commons.Exceptions;
using LumiaFoundation.AspNetCore.ExceptionHandlers.ErrorModel;
using LumiaFoundation.Logger.Contracts;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;

namespace LumiaFoundation.AspNetCore.ExceptionHandlers
{
    public class UnhandledExceptionHandler(ILoggerManager logger) : IExceptionHandler
    {
        private readonly ILoggerManager _logger = logger;

        public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
        {
            if (exception is DomainBaseException)
            {
                return false;
            }

            httpContext.Response.ContentType = "application/json";
            await HandleExceptionAsync(httpContext, _logger, exception, cancellationToken);

            return true;
        }

        private static async Task HandleExceptionAsync(HttpContext context, ILoggerManager logger, Exception exception, CancellationToken cancellationToken)
        {
            LogException(logger, exception);
            var errorDetails = CreateErrorDetails(exception);
            context.Response.StatusCode = errorDetails.StatusCode;
            await context.Response.WriteAsync(errorDetails.ToString(), cancellationToken);
        }

        private static ErrorDetails CreateErrorDetails(Exception exception) => new()
        {
            StatusCode = StatusCodes.Status500InternalServerError,
            Message = "Internal Server Error.",
            ExceptionType = nameof(exception)
        };

        private static void LogException(ILoggerManager logger, Exception exception)
        {
            logger.LogError(exception, $"Ocorreu um erro desconhecido: {exception}");
        }
    }
}