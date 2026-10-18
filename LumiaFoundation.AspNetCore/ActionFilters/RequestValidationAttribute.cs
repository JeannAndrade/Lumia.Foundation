using LumiaFoundation.AspNetCore.Commons.Exceptions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Filters;

namespace LumiaFoundation.AspNetCore.ActionFilters;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public sealed class RequestValidationAttribute : ActionFilterAttribute
{
    public override void OnActionExecuting(ActionExecutingContext context)
    {
        foreach (var parameter in context.ActionDescriptor.Parameters
                     .Where(parameter => parameter.BindingInfo?.BindingSource == BindingSource.Body))
        {
            if (!context.ActionArguments.TryGetValue(parameter.Name, out var argument) || argument is null)
                throw new HttpBaseException(StatusCodes.Status422UnprocessableEntity, "Corpo da requisição inválido.");
        }

        if (!context.ModelState.IsValid)
        {
            var keys = context.ModelState
                .Where(entry => entry.Value?.Errors.Count > 0)
                .Select(entry => entry.Key)
                .OrderBy(key => key);
            throw new HttpBaseException(StatusCodes.Status422UnprocessableEntity,
                $"Parâmetros inválidos: {string.Join(", ", keys)}.");
        }
    }
}
