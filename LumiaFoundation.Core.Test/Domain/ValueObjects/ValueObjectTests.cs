using LumiaFoundation.Core.Domain.ValueObjects;

namespace LumiaFoundation.Core.Test.Domain.ValueObjects;

public class ValueObjectTests
{
    [Fact]
    public void EqualityOperator_WhenBothAreNull_ReturnsTrue()
    {
        // Arrange
        TestValueObject? left = null;
        TestValueObject? right = null;

        // Act
        var result = left == right;

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void EqualityOperator_WhenOnlyOneIsNull_ReturnsFalse()
    {
        // Arrange
        TestValueObject? left = new(1, "a");
        TestValueObject? right = null;

        // Act
        var result = left != right;

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void Equals_WhenComponentsMatch_ReturnsTrue()
    {
        // Arrange
        var left = new TestValueObject(1, "a");
        var right = new TestValueObject(1, "a");

        // Act
        var result = left.Equals(right);

        // Assert
        Assert.True(result);
        Assert.Equal(left.GetHashCode(), right.GetHashCode());
    }

    private sealed class TestValueObject(int number, string text) : ValueObject
    {
        protected override IEnumerable<object> GetEqualityComponents()
        {
            yield return number;
            yield return text;
        }
    }
}
