using LumiaFoundation.Auth.Services;
using LumiaFoundation.Logger.Contracts;
using Microsoft.AspNetCore.Mvc.Filters;

namespace LumiaFoundation.Auth.ActionFilters;

public class RetrieveUserIdFromTokenAttribute(IJwtTokenService jwtTokenService, ILoggerManager logger) : IActionFilter
{
    public void OnActionExecuting(ActionExecutingContext context)
    {
        var token = context.HttpContext.Request.Headers.Authorization.ToString() ?? "";
        var userId = jwtTokenService.GetUserIdFromToken(token.Replace("Bearer ", ""));

        context.HttpContext.Items.Add("UserId", userId);
        logger.LogInfo($"userId = {userId}");
    }

    public void OnActionExecuted(ActionExecutedContext context) { }
}
