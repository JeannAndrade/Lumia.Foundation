using System.Text.Json;
using LumiaFoundation.AspNetCore.Commons.Exceptions;
using LumiaFoundation.AspNetCore.ExceptionHandlers;
using LumiaFoundation.Abstractions.ErrorModel;
using LumiaFoundation.AspNetCore.Test.TestDoubles;
using Microsoft.AspNetCore.Http;

namespace LumiaFoundation.AspNetCore.Test.ExceptionHandlers;

public class DomainExceptionHandlerTests
{
    [Fact]
    public async Task TryHandleAsync_WhenExceptionIsNotHttpBaseException_ReturnsFalse()
    {
        // Arrange
        var logger = new FakeLoggerManager();
        var handler = new DomainExceptionHandler(logger);
        var context = new DefaultHttpContext();

        // Act
        var result = await handler.TryHandleAsync(context, new InvalidOperationException("boom"), CancellationToken.None);

        // Assert
        Assert.False(result);
        Assert.Empty(logger.ErrorMessages);
        Assert.Empty(logger.WarnMessages);
    }

    [Fact]
    public async Task TryHandleAsync_WhenExceptionIs4xxHttpBaseException_WritesErrorDetailsAndLogsWarning()
    {
        // Arrange
        var logger = new FakeLoggerManager();
        var handler = new DomainExceptionHandler(logger);
        var context = new DefaultHttpContext();
        context.Response.Body = new MemoryStream();

        // Act
        var handled = await handler.TryHandleAsync(context, new TestHttpBaseException(404, "Missing"), CancellationToken.None);

        // Assert
        context.Response.Body.Position = 0;
        var body = await new StreamReader(context.Response.Body).ReadToEndAsync();
        var error = JsonSerializer.Deserialize<ErrorDetails>(body);

        Assert.True(handled);
        Assert.Equal("application/json", context.Response.ContentType);
        Assert.Equal(404, context.Response.StatusCode);
        Assert.NotNull(error);
        Assert.Equal(404, error.StatusCode);
        Assert.Equal("Missing", error.Message);
        Assert.Equal(nameof(TestHttpBaseException), error.ExceptionType);
        Assert.Single(logger.WarnMessages);
        Assert.Empty(logger.ErrorMessages);
    }

    [Fact]
    public async Task TryHandleAsync_WhenExceptionIs5xxHttpBaseException_WritesErrorDetailsAndLogsError()
    {
        // Arrange
        var logger = new FakeLoggerManager();
        var handler = new DomainExceptionHandler(logger);
        var context = new DefaultHttpContext();
        context.Response.Body = new MemoryStream();

        // Act
        var handled = await handler.TryHandleAsync(context, new TestHttpBaseException(500, "Boom"), CancellationToken.None);

        // Assert
        context.Response.Body.Position = 0;
        var body = await new StreamReader(context.Response.Body).ReadToEndAsync();
        var error = JsonSerializer.Deserialize<ErrorDetails>(body);

        Assert.True(handled);
        Assert.Equal(500, context.Response.StatusCode);
        Assert.NotNull(error);
        Assert.Equal(500, error.StatusCode);
        Assert.Equal("Boom", error.Message);
        Assert.Single(logger.ErrorMessages);
        Assert.Empty(logger.WarnMessages);
    }

    private sealed class TestHttpBaseException(int statusCode, string message) : HttpBaseException(statusCode, message)
    {
    }
}
