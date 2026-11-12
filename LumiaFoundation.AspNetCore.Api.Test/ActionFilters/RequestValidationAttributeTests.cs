using LumiaFoundation.AspNetCore.ActionFilters;
using LumiaFoundation.AspNetCore.Commons.Exceptions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Routing;

namespace LumiaFoundation.AspNetCore.Test.ActionFilters;

public class RequestValidationAttributeTests
{
    [Fact]
    public void NullBodyThrowsBodyMessage()
    {
        var exception = Assert.Throws<HttpBaseException>(() => new RequestValidationAttribute().OnActionExecuting(CreateContext(null, body: true)));
        Assert.Equal(StatusCodes.Status422UnprocessableEntity, exception.StatusCode);
        Assert.Equal("Corpo da requisição inválido.", exception.Message);
    }

    [Fact]
    public void ValidBodyDoesNothing() => Assert.Null(CreateAndExecute(new object(), body: true).Result);

    [Fact]
    public void InvalidModelStateWithoutBodyListsOnlyKeys()
    {
        var context = CreateContext("secret-value");
        context.ModelState.AddModelError("Name", "required");
        context.ModelState.AddModelError("Age", "invalid");

        var exception = Assert.Throws<HttpBaseException>(() => new RequestValidationAttribute().OnActionExecuting(context));
        Assert.Equal("Parâmetros inválidos: Age, Name.", exception.Message);
        Assert.DoesNotContain("secret-value", exception.Message);
    }

    [Fact]
    public void NullBodyTakesPrecedenceOverInvalidModelState()
    {
        var context = CreateContext(null, body: true);
        context.ModelState.AddModelError("Name", "required");
        var exception = Assert.Throws<HttpBaseException>(() => new RequestValidationAttribute().OnActionExecuting(context));
        Assert.Equal("Corpo da requisição inválido.", exception.Message);
    }

    [Fact]
    public void RouteGuidAndFilledBodyDoNothing() => Assert.Null(CreateAndExecute(new object(), body: true, routeGuid: true).Result);

    [Fact]
    public void ActionWithoutParametersDoesNothing() => Assert.Null(CreateAndExecute(null).Result);

    private static ActionExecutingContext CreateAndExecute(object? argument, bool body = false, bool routeGuid = false)
    {
        var context = CreateContext(argument, body, routeGuid);
        new RequestValidationAttribute().OnActionExecuting(context);
        return context;
    }

    private static ActionExecutingContext CreateContext(object? argument, bool body = false, bool routeGuid = false)
    {
        var descriptor = new ControllerActionDescriptor { Parameters = new List<ParameterDescriptor>() };
        if (body)
            descriptor.Parameters.Add(new ControllerParameterDescriptor { Name = "request", ParameterInfo = typeof(TestController).GetMethod(nameof(TestController.Action))!.GetParameters()[0], BindingInfo = new BindingInfo { BindingSource = BindingSource.Body } });
        if (routeGuid)
            descriptor.Parameters.Add(new ControllerParameterDescriptor { Name = "id", ParameterInfo = typeof(TestController).GetMethod(nameof(TestController.ActionWithRoute))!.GetParameters()[0], BindingInfo = new BindingInfo { BindingSource = BindingSource.Path } });

        var actionContext = new ActionContext(new DefaultHttpContext(), new RouteData(), descriptor);
        var arguments = new Dictionary<string, object?>();
        if (body) arguments["request"] = argument;
        if (routeGuid) arguments["id"] = Guid.NewGuid();
        return new ActionExecutingContext(actionContext, new List<IFilterMetadata>(), arguments, new TestController());
    }

    private sealed class TestController
    {
        public void Action(object request) { }
        public void ActionWithRoute(Guid id) { }
    }
}
