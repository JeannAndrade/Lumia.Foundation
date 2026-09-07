using LumiaFoundation.AspNetCore.Auth.Exceptions;

namespace LumiaFoundation.AspNetCore.Test.Auth.Exceptions;

public class RefreshTokenBadRequestTests
{
    [Fact]
    public void Constructor_SetsExpectedMessage()
    {
        // Arrange
        const string expectedMessage = "Invalid client request. The tokenDto has some invalid values.";

        // Act
        var exception = new RefreshTokenBadRequest();

        // Assert
        Assert.Equal(expectedMessage, exception.Message);
    }
}
