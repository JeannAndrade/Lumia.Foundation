using LumiaFoundation.Core.Domain.Exceptions;

namespace LumiaFoundation.Core.Test.Domain.Exceptions;

public class EntityInUseExceptionTests
{
    [Fact]
    public void Constructor_SetsMessage()
    {
        // Arrange
        const string message = "Objetivo possui Movimentos associados";

        // Act
        var exception = new EntityInUseException(message);

        // Assert
        Assert.Equal(message, exception.Message);
    }
}
