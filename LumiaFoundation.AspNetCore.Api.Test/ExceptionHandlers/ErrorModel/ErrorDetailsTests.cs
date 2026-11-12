using System.Text.Json;
using LumiaFoundation.Abstractions.ErrorModel;

namespace LumiaFoundation.AspNetCore.Test.ExceptionHandlers.ErrorModel;

public class ErrorDetailsTests
{
    [Fact]
    public void ToString_ReturnsSerializedJson()
    {
        // Arrange
        var errorDetails = new ErrorDetails
        {
            StatusCode = 500,
            Message = "Internal Server Error.",
            ExceptionType = "InvalidOperationException"
        };

        // Act
        var result = errorDetails.ToString();
        var deserialized = JsonSerializer.Deserialize<ErrorDetails>(result);

        // Assert
        Assert.Contains("\"StatusCode\":500", result);
        Assert.Contains("\"Message\":\"Internal Server Error.\"", result);
        Assert.Contains("\"ExceptionType\":\"InvalidOperationException\"", result);
        Assert.NotNull(deserialized);
        Assert.Equal(errorDetails.StatusCode, deserialized.StatusCode);
        Assert.Equal(errorDetails.Message, deserialized.Message);
        Assert.Equal(errorDetails.ExceptionType, deserialized.ExceptionType);
    }
}
