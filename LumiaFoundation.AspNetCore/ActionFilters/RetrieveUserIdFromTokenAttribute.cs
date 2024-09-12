using LumiaFoundation.AspNetCore.Auth.Services;
using Microsoft.AspNetCore.Mvc.Filters;

namespace LumiaFoundation.AspNetCore.ActionFilters
{
    public class RetrieveUserIdFromTokenAttribute(IJwtTokenService jwtTokenService) : IActionFilter
    {
        public void OnActionExecuting(ActionExecutingContext context)
        {
            var token = context.HttpContext.Request.Headers.Authorization.ToString() ?? "";
            var userId = jwtTokenService.GetUserIdFromToken(token.Replace("Bearer ", ""));

            context.HttpContext.Items.Add("UserId", userId);
        }

        public void OnActionExecuted(ActionExecutedContext context) { }
    }
}

