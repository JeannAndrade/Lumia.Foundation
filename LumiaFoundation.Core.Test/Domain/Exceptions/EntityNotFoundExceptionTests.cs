using LumiaFoundation.Core.Domain.Exceptions;

namespace LumiaFoundation.Core.Test.Domain.Exceptions;

public class EntityNotFoundExceptionTests
{
    [Fact]
    public void Constructor_SetsMessage()
    {
        // Arrange
        const string message = "Entity not found";

        // Act
        var exception = new EntityNotFoundException(message);

        // Assert
        Assert.Equal(message, exception.Message);
    }
}
