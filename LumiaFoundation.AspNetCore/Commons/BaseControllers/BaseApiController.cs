using LumiaFoundation.AspNetCore.Commons.Exceptions;
using LumiaFoundation.Core.Domain.Exceptions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace LumiaFoundation.AspNetCore.Commons.BaseControllers;

public abstract class BaseApiController : ControllerBase, IAsyncExceptionFilter
{
    [NonAction]
    public Task OnExceptionAsync(ExceptionContext context)
    {
        context.Exception = context.Exception switch
        {
            HttpBaseException => context.Exception,
            DomainBaseException domainEx => new HttpBaseException(
                GetStatusCode(domainEx), domainEx.Message, domainEx),
            var ex => new HttpBaseException(
                StatusCodes.Status500InternalServerError, ex.Message, ex)
        };

        return Task.CompletedTask;
    }

    private static int GetStatusCode(DomainBaseException exception) => exception switch
    {
        EntityNotFoundException => StatusCodes.Status404NotFound,
        CommandValidationException => StatusCodes.Status422UnprocessableEntity,
        _ => StatusCodes.Status500InternalServerError
    };
}