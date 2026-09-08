using LumiaFoundation.AspNetCore.Commons.Exceptions;
using LumiaFoundation.Abstractions.ErrorModel;
using LumiaFoundation.Logger.Contracts;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;

namespace LumiaFoundation.AspNetCore.ExceptionHandlers;

public class DomainExceptionHandler(ILoggerManager logger) : IExceptionHandler
{
    private readonly ILoggerManager _logger = logger;

    public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    {
        if (exception is not HttpBaseException domainException)
        {
            return false;
        }

        httpContext.Response.ContentType = "application/json";
        await HandleExceptionAsync(httpContext, _logger, domainException, cancellationToken);

        return true;
    }

    private static async Task HandleExceptionAsync(HttpContext context, ILoggerManager logger, HttpBaseException exception, CancellationToken cancellationToken)
    {
        LogDomainException(logger, exception);
        var errorDetails = CreateErrorDetails(exception);
        context.Response.StatusCode = errorDetails.StatusCode;
        await context.Response.WriteAsync(errorDetails.ToString(), cancellationToken);
    }

    private static ErrorDetails CreateErrorDetails(HttpBaseException exception) => new()
    {
        StatusCode = exception.StatusCode,
        Message = exception.Message,
        ExceptionType = exception.GetType().Name
    };

    private static void LogDomainException(ILoggerManager logger, HttpBaseException exception)
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
