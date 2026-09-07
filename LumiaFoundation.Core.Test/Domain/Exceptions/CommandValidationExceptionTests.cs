using LumiaFoundation.Core.Domain.Exceptions;

namespace LumiaFoundation.Core.Test.Domain.Exceptions;

public class CommandValidationExceptionTests
{
    [Fact]
    public void Constructor_SetsMessage()
    {
        // Arrange
        const string message = "Validation failed";

        // Act
        var exception = new CommandValidationException(message);

        // Assert
        Assert.Equal(message, exception.Message);
    }
}
