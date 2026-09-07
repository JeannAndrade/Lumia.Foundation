using LumiaFoundation.EFRepository.Utils;

namespace LumiaFoundation.EFRepository.Test.Utils;

public class DbConnectionHelperTests
{
    [Fact]
    public void GetConectionString_ReturnsExpectedConnectionString()
    {
        // Arrange
        var helper = new DbConnectionHelper("localhost", "3306", "user", "password", "database", 10, 6, 12);

        // Act
        var result = helper.GetConectionString();

        // Assert
        Assert.Equal("server=localhost;port=3306;user=user;password=password;database=database", result);
    }

    [Fact]
    public void VersionProperties_ReturnConfiguredValues()
    {
        // Arrange
        var helper = new DbConnectionHelper("localhost", "3306", "user", "password", "database", 10, 6, 12);

        // Assert
        Assert.Equal(10, helper.MajorVersion);
        Assert.Equal(6, helper.MinorVersion);
        Assert.Equal(12, helper.BuildVersion);
    }
}
