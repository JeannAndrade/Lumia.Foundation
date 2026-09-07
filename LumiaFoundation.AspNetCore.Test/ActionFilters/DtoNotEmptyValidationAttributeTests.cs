using LumiaFoundation.AspNetCore.ActionFilters;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Routing;

namespace LumiaFoundation.AspNetCore.Test.ActionFilters;

public class DtoNotEmptyValidationAttributeTests
{
    [Fact]
    public void OnActionExecuting_WhenActionArgumentsAreEmpty_ReturnsBadRequest()
    {
        // Arrange
        var attribute = new DtoNotEmptyValidationAttribute();
        var context = CreateContext();

        // Act
        attribute.OnActionExecuting(context);

        // Assert
        var result = Assert.IsType<BadRequestObjectResult>(context.Result);
        Assert.Contains("Body is empty", result.Value?.ToString());
    }

    [Fact]
    public void OnActionExecuting_WhenNoDtoArgumentExists_ReturnsBadRequest()
    {
        // Arrange
        var attribute = new DtoNotEmptyValidationAttribute();
        var context = CreateContext(new Dictionary<string, object?>
        {
            ["payload"] = new { Value = "not a dto" }
        });

        // Act
        attribute.OnActionExecuting(context);

        // Assert
        var result = Assert.IsType<BadRequestObjectResult>(context.Result);
        Assert.Contains("Body not found", result.Value?.ToString());
    }

    [Fact]
    public void OnActionExecuting_WhenModelStateIsInvalid_ReturnsUnprocessableEntity()
    {
        // Arrange
        var attribute = new DtoNotEmptyValidationAttribute();
        var context = CreateContext(new Dictionary<string, object?>
        {
            ["dto"] = new SampleDto()
        });
        context.ModelState.AddModelError("Name", "Required");

        // Act
        attribute.OnActionExecuting(context);

        // Assert
        Assert.IsType<UnprocessableEntityObjectResult>(context.Result);
    }

    [Fact]
    public void OnActionExecuting_WhenDtoIsPresentAndModelStateIsValid_DoesNotSetResult()
    {
        // Arrange
        var attribute = new DtoNotEmptyValidationAttribute();
        var context = CreateContext(new Dictionary<string, object?>
        {
            ["dto"] = new SampleDto()
        });

        // Act
        attribute.OnActionExecuting(context);

        // Assert
        Assert.Null(context.Result);
    }

    private static ActionExecutingContext CreateContext(IDictionary<string, object?>? actionArguments = null)
    {
        var httpContext = new DefaultHttpContext();
        var actionContext = new Microsoft.AspNetCore.Mvc.ActionContext(httpContext, new RouteData
        {
            Values =
            {
                ["controller"] = "Home",
                ["action"] = "Index"
            }
        }, new ActionDescriptor());

        return new ActionExecutingContext(
            actionContext,
            new List<IFilterMetadata>(),
            actionArguments ?? new Dictionary<string, object?>(),
            controller: new object());
    }

    private sealed class SampleDto
    {
        public string? Name { get; set; }
    }
}
