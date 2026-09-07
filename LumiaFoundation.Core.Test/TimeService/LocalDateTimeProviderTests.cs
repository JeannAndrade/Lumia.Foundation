using LumiaFoundation.Core.TimeService;

namespace LumiaFoundation.Core.Test.TimeService;

public class LocalDateTimeProviderTests
{
    [Fact]
    public void GetDateTime_ReturnsLocalNow()
    {
        // Arrange
        var provider = new LocalDateTimeProvider();
        var before = DateTime.Now;

        // Act
        var result = provider.GetDateTime();
        var after = DateTime.Now;

        // Assert
        Assert.InRange(result, before.AddSeconds(-1), after.AddSeconds(1));
        Assert.Equal(DateTimeKind.Local, result.Kind);
    }
}
