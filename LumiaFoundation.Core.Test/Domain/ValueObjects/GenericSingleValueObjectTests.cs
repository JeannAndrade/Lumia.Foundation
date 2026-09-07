using LumiaFoundation.Core.Domain.ValueObjects;

namespace LumiaFoundation.Core.Test.Domain.ValueObjects;

public class GenericSingleValueObjectTests
{
    [Fact]
    public void Constructor_WhenValueIsNull_ThrowsArgumentNullException()
    {
        // Arrange
        string? value = null;

        // Act
        var exception = Assert.Throws<ArgumentNullException>(() => new TestValueObject(value!));

        // Assert
        Assert.Equal("value", exception.ParamName);
    }

    [Fact]
    public void ImplicitConversion_FromValueAndBack_PreservesValue()
    {
        // Arrange
        const string value = "abc";

        // Act
        GenericSingleValueObject<string> valueObject = value;
        string result = valueObject;

        // Assert
        Assert.Equal(value, result);
    }

    [Fact]
    public void Equality_WhenValuesMatch_ReturnsTrue()
    {
        // Arrange
        var left = new TestValueObject("same");
        var right = new TestValueObject("same");

        // Act
        var areEqual = left == right;

        // Assert
        Assert.True(areEqual);
        Assert.Equal(left, right);
    }

    private sealed class TestValueObject : GenericSingleValueObject<string>
    {
        public TestValueObject(string value) : base(value)
        {
        }
    }
}
