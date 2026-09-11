using System.Net;
using LumiaFoundation.Http.Client.Exceptions;

namespace LumiaFoundation.Http.Client.Test.Exceptions;

public class ApiExceptionTests
{
    [Fact]
    public void Constructor_SetsStatusCodeAndMessage()
    {
        // Arrange
        const string message = "Rate limited";

        // Act
        var exception = new ApiException(HttpStatusCode.TooManyRequests, message);

        // Assert
        Assert.Equal(HttpStatusCode.TooManyRequests, exception.StatusCode);
        Assert.Equal(message, exception.Message);
    }
}
