using LumiaFoundation.AspNetCore.ActionFilters;
using LumiaFoundation.AspNetCore.Auth.Services;
using LumiaFoundation.AspNetCore.Test.TestDoubles;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Routing;

namespace LumiaFoundation.AspNetCore.Test.ActionFilters;

public class RetrieveUserIdFromTokenAttributeTests
{
    [Fact]
    public void OnActionExecuting_AddsUserIdToHttpContextItemsAndLogsIt()
    {
        // Arrange
        var jwtTokenService = new FakeJwtTokenService("user-123");
        var logger = new FakeLoggerManager();
        var attribute = new RetrieveUserIdFromTokenAttribute(jwtTokenService, logger);
        var context = CreateContext();
        context.HttpContext.Request.Headers.Authorization = "Bearer fake-token";

        // Act
        attribute.OnActionExecuting(context);

        // Assert
        Assert.True(context.HttpContext.Items.ContainsKey("UserId"));
        Assert.Equal("user-123", context.HttpContext.Items["UserId"]);
        Assert.Single(logger.InfoMessages);
        Assert.Contains("userId = user-123", logger.InfoMessages.Single());
    }

    private static ActionExecutingContext CreateContext()
    {
        var httpContext = new DefaultHttpContext();
        var actionContext = new Microsoft.AspNetCore.Mvc.ActionContext(httpContext, new RouteData(), new ActionDescriptor());

        return new ActionExecutingContext(actionContext, new List<IFilterMetadata>(), new Dictionary<string, object?>(), new object());
    }
}
