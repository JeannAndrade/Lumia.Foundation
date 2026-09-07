using LumiaFoundation.Core.TimeService;

namespace LumiaFoundation.Core.Test.TimeService;

public class UtcDateTimeProviderTests
{
    [Fact]
    public void GetDateTime_ReturnsUtcNow()
    {
        // Arrange
        var provider = new UtcDateTimeProvider();
        var before = DateTime.UtcNow;

        // Act
        var result = provider.GetDateTime();
        var after = DateTime.UtcNow;

        // Assert
        Assert.InRange(result, before.AddSeconds(-1), after.AddSeconds(1));
        Assert.Equal(DateTimeKind.Utc, result.Kind);
    }
}
