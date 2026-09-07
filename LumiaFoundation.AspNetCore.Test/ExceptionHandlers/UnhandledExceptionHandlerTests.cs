using System.Text.Json;
using LumiaFoundation.AspNetCore.ExceptionHandlers;
using LumiaFoundation.AspNetCore.ExceptionHandlers.ErrorModel;
using LumiaFoundation.AspNetCore.Test.TestDoubles;
using Microsoft.AspNetCore.Http;

namespace LumiaFoundation.AspNetCore.Test.ExceptionHandlers;

public class UnhandledExceptionHandlerTests
{
    [Fact]
    public async Task TryHandleAsync_WhenExceptionIsHttpBaseException_ReturnsFalse()
    {
        // Arrange
        var logger = new FakeLoggerManager();
        var handler = new UnhandledExceptionHandler(logger);
        var context = new DefaultHttpContext();

        // Act
        var result = await handler.TryHandleAsync(context, new TestHttpBaseException(400, "bad"), CancellationToken.None);

        // Assert
        Assert.False(result);
        Assert.Empty(logger.ErrorMessages);
    }

    [Fact]
    public async Task TryHandleAsync_WhenExceptionIsUnhandled_WritesInternalServerErrorAndLogsError()
    {
        // Arrange
        var logger = new FakeLoggerManager();
        var handler = new UnhandledExceptionHandler(logger);
        var context = new DefaultHttpContext();
        context.Response.Body = new MemoryStream();
        var exception = new InvalidOperationException("boom");

        // Act
        var handled = await handler.TryHandleAsync(context, exception, CancellationToken.None);

        // Assert
        context.Response.Body.Position = 0;
        var body = await new StreamReader(context.Response.Body).ReadToEndAsync();
        var error = JsonSerializer.Deserialize<ErrorDetails>(body);

        Assert.True(handled);
        Assert.Equal("application/json", context.Response.ContentType);
        Assert.Equal(StatusCodes.Status500InternalServerError, context.Response.StatusCode);
        Assert.NotNull(error);
        Assert.Equal(StatusCodes.Status500InternalServerError, error.StatusCode);
        Assert.Equal("Internal Server Error.", error.Message);
        Assert.Equal(nameof(InvalidOperationException), error.ExceptionType);
        Assert.Single(logger.ErrorMessages);
        Assert.Same(exception, logger.Exceptions.Single());
    }

    private sealed class TestHttpBaseException(int statusCode, string message) : LumiaFoundation.AspNetCore.Commons.Exceptions.HttpBaseException(statusCode, message)
    {
    }
}
