using LumiaFoundation.Logger.LoggerService;

namespace LumiaFoundation.Logger.Test.LoggerService;

public class LoggerManagerTests
{
    [Fact]
    public void LoadConfigurationFromFile_WhenFileExists_DoesNotThrow()
    {
        // Arrange
        var path = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid():N}.config");
        File.WriteAllText(path, """
        <nlog xmlns="http://www.nlog-project.org/schemas/NLog.xsd"
              xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance">
        </nlog>
        """);

        try
        {
            // Act
            var exception = Record.Exception(() => LoggerManager.LoadConfigurationFromFile(path));

            // Assert
            Assert.Null(exception);
        }
        finally
        {
            File.Delete(path);
        }
    }

    [Fact]
    public void LoadConfigurationFromFile_WhenFileDoesNotExist_DoesNotThrow()
    {
        // Arrange
        var path = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid():N}.config");

        // Act
        var exception = Record.Exception(() => LoggerManager.LoadConfigurationFromFile(path));

        // Assert
        Assert.Null(exception);
    }
}
