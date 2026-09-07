using LumiaFoundation.AspNetCore.Commons.Exceptions;
using LumiaFoundation.AspNetCore.ServiceFilters;
using LumiaFoundation.Core.Domain.Exceptions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Routing;

namespace LumiaFoundation.AspNetCore.Test.Commons.BaseControllers;

public class BaseApiControllerTests
{
    [Fact]
    public void OnException_WhenExceptionIsDomainException_WrapsItInHttpBaseException()
    {
        // Arrange
        var filter = new DomainExceptionMappingFilter();
        var exceptionContext = CreateExceptionContext(new EntityNotFoundException("missing"));

        // Act
        filter.OnException(exceptionContext);

        // Assert
        var exception = Assert.IsType<HttpBaseException>(exceptionContext.Exception);
        Assert.Equal(StatusCodes.Status404NotFound, exception.StatusCode);
        Assert.Equal("missing", exception.Message);
        Assert.IsType<EntityNotFoundException>(exception.InnerException);
    }

    [Fact]
    public void OnException_WhenExceptionIsAlreadyHttpBaseException_KeepsOriginalException()
    {
        // Arrange
        var filter = new DomainExceptionMappingFilter();
        var original = new TestHttpBaseException(400, "bad");
        var exceptionContext = CreateExceptionContext(original);

        // Act
        filter.OnException(exceptionContext);

        // Assert
        Assert.Same(original, exceptionContext.Exception);
    }

    [Fact]
    public void OnException_WhenExceptionIsUnexpected_WrapsItIn500HttpBaseException()
    {
        // Arrange
        var filter = new DomainExceptionMappingFilter();
        var exceptionContext = CreateExceptionContext(new InvalidOperationException("boom"));

        // Act
        filter.OnException(exceptionContext);

        // Assert
        var exception = Assert.IsType<HttpBaseException>(exceptionContext.Exception);
        Assert.Equal(StatusCodes.Status500InternalServerError, exception.StatusCode);
        Assert.Equal("boom", exception.Message);
        Assert.IsType<InvalidOperationException>(exception.InnerException);
    }

    private static ExceptionContext CreateExceptionContext(Exception exception)
    {
        var httpContext = new DefaultHttpContext();
        var actionContext = new Microsoft.AspNetCore.Mvc.ActionContext(httpContext, new RouteData(), new ActionDescriptor());
        return new ExceptionContext(actionContext, new List<IFilterMetadata>()) { Exception = exception };
    }

    private sealed class TestHttpBaseException(int statusCode, string message) : HttpBaseException(statusCode, message)
    {
    }
}
