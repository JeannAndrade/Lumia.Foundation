using LumiaFoundation.AspNetCore.Commons.Exceptions;
using LumiaFoundation.Core.Domain.Exceptions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace LumiaFoundation.AspNetCore.Commons.BaseControllers;

public abstract class BaseApiController : ControllerBase, IAsyncActionFilter
{
    public async Task OnActionExecutionAsync(
        ActionExecutingContext context,
        ActionExecutionDelegate next)
    {
        try
        {
            await next(); // dispara o restante da pipeline: outros filters + a Action em si
        }
        catch (HttpBaseException)
        {
            throw;
        }
        catch (DomainBaseException ex)
        {
            throw new HttpBaseException(GetStatusCode(ex), ex.Message, ex);
        }
        catch (Exception ex)
        {
            throw new HttpBaseException(StatusCodes.Status500InternalServerError, ex.Message, ex);
        }
    }

    private static int GetStatusCode(DomainBaseException exception) => exception switch
    {
        EntityNotFoundException => StatusCodes.Status404NotFound,
        CommandValidationException => StatusCodes.Status422UnprocessableEntity,
        _ => StatusCodes.Status500InternalServerError
    };
}