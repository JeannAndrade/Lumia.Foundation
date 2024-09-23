using System.Net;
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
                    var contextFeature = context.Features.Get<IExceptionHandlerFeature>();

                    if (contextFeature != null)
                    {
                        context.Response.ContentType = "application/json";

                        if (contextFeature.Error is DomainBaseException)
                        {
                            logger.LogError($"Ocorreu um erro de validação: {contextFeature.Error}");
                            context.Response.StatusCode = StatusCodes.Status422UnprocessableEntity;
                            await context.Response.WriteAsync(new ErrorDetails()
                            {
                                StatusCode = context.Response.StatusCode,
                                Message = contextFeature.Error.Message,
                                ExceptionType = contextFeature.Error.GetType().Name
                            }.ToString());
                        }
                        else
                        {
                            logger.LogError(contextFeature.Error, $"Ocorreu um erro desconhecido: {contextFeature.Error}");
                            context.Response.StatusCode = StatusCodes.Status500InternalServerError;
                            await context.Response.WriteAsync(new ErrorDetails()
                            {
                                StatusCode = context.Response.StatusCode,
                                Message = "Internal Server Error.",
                                ExceptionType = nameof(contextFeature.Error)
                            }.ToString());
                        }
                    }
                });
            });
        }
    }
}