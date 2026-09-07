using LumiaFoundation.Core.Utils;

namespace LumiaFoundation.Core.Test.Utils;

public class ParseHelperTests
{
    [Fact]
    public void ToIntOrDefault_WhenValueIsNumeric_ReturnsParsedInteger()
    {
        // Arrange
        const string value = "42";
        const int defaultValue = 7;

        // Act
        var result = ParseHelper.ToIntOrDefault(value, defaultValue);

        // Assert
        Assert.Equal(42, result);
    }

    [Theory]
    [InlineData("abc")]
    [InlineData("")]
    [InlineData(null)]
    public void ToIntOrDefault_WhenValueIsInvalid_ReturnsDefaultValue(string? value)
    {
        // Arrange
        const int defaultValue = 7;

        // Act
        var result = ParseHelper.ToIntOrDefault(value!, defaultValue);

        // Assert
        Assert.Equal(defaultValue, result);
    }
}
