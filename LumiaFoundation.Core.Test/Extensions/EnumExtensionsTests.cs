using System.ComponentModel;
using LumiaFoundation.Core.Extensions;

namespace LumiaFoundation.Core.Test.Extensions;

public class EnumExtensionsTests
{
    private enum SampleEnum
    {
        [Description("Item com descrição")]
        Described,
        Plain
    }

    [Fact]
    public void GetDescription_WhenEnumHasDescriptionAttribute_ReturnsDescription()
    {
        // Arrange
        var value = SampleEnum.Described;

        // Act
        var result = value.GetDescription();

        // Assert
        Assert.Equal("Item com descrição", result);
    }

    [Fact]
    public void GetDescription_WhenEnumDoesNotHaveDescriptionAttribute_ReturnsEnumName()
    {
        // Arrange
        var value = SampleEnum.Plain;

        // Act
        var result = value.GetDescription();

        // Assert
        Assert.Equal("Plain", result);
    }
}
