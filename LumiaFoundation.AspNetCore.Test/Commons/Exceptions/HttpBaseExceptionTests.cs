using LumiaFoundation.AspNetCore.Commons.Exceptions;

namespace LumiaFoundation.AspNetCore.Test.Commons.Exceptions;

public class HttpBaseExceptionTests
{
    [Fact]
    public void Constructor_WithStatusCodeAndMessage_SetsProperties()
    {
        // Arrange
        const int statusCode = 422;
        const string message = "Validation failed";

        // Act
        var exception = new TestHttpBaseException(statusCode, message);

        // Assert
        Assert.Equal(statusCode, exception.StatusCode);
        Assert.Equal(message, exception.Message);
    }

    [Fact]
    public void Constructor_WithInnerException_SetsInnerException()
    {
        // Arrange
        var inner = new InvalidOperationException("inner");

        // Act
        var exception = new TestHttpBaseException(500, "message", inner);

        // Assert
        Assert.Same(inner, exception.InnerException);
    }

    private sealed class TestHttpBaseException : HttpBaseException
    {
        public TestHttpBaseException(int statusCode, string message) : base(statusCode, message)
        {
        }

        public TestHttpBaseException(int statusCode, string message, Exception inner) : base(statusCode, message, inner)
        {
        }
    }
}
