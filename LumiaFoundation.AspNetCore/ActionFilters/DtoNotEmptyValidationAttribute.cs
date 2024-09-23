using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace LumiaFoundation.AspNetCore.ActionFilters
{
    public class DtoNotEmptyValidationAttribute : IActionFilter
    {
        public DtoNotEmptyValidationAttribute() { }
        public void OnActionExecuting(ActionExecutingContext context)
        {
            var action = context.RouteData.Values["action"];
            var controller = context.RouteData.Values["controller"];
            var actionArguments = context.ActionArguments.Values;

            if (actionArguments == null || actionArguments.Count == 0)
            {
                context.Result = new BadRequestObjectResult($"Body is empty. Controller: {controller}, action: {action}");
                return;
            }
            else
            {
#pragma warning disable CS8602 // Dereference of a possibly null reference.
                var param = actionArguments.SingleOrDefault(x => x != null && x.ToString().Contains("Dto"));
#pragma warning restore CS8602 // Dereference of a possibly null reference.
                if (param is null)
                {
                    context.Result = new BadRequestObjectResult($"Body not found. Controller: {controller}, action: {action}");
                    return;
                }
            }

            if (!context.ModelState.IsValid)
                context.Result = new UnprocessableEntityObjectResult(context.ModelState);
        }
        public void OnActionExecuted(ActionExecutedContext context) { }
    }
}