using LumiaFoundation.EFRepository.Extensions;
using LumiaFoundation.EFRepository.Test.Repository;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Options;

namespace LumiaFoundation.EFRepository.Test.Extensions;

public class HealthChecksBuilderExtensionsTests
{
    [Fact]
    public void AddDbContextHealthCheck_WithDefaults_RegistersCheckWithDefaultValues()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.AddHealthChecks().AddDbContextHealthCheck<TestRepositoryContext>();
        using var provider = services.BuildServiceProvider();
        var registration = Assert.Single(
            provider.GetRequiredService<IOptions<HealthCheckServiceOptions>>().Value.Registrations);

        // Assert
        Assert.Equal("banco-de-dados", registration.Name);
        Assert.Equal(TimeSpan.FromSeconds(5), registration.Timeout);
        Assert.Equal(HealthStatus.Unhealthy, registration.FailureStatus);
        Assert.Empty(registration.Tags);
    }

    [Fact]
    public void AddDbContextHealthCheck_WithCustomValues_RegistersCheckWithGivenValues()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.AddHealthChecks().AddDbContextHealthCheck<TestRepositoryContext>(
            name: "meu-banco",
            failureStatus: HealthStatus.Degraded,
            tags: ["ready"],
            timeout: TimeSpan.FromSeconds(2));
        using var provider = services.BuildServiceProvider();
        var registration = Assert.Single(
            provider.GetRequiredService<IOptions<HealthCheckServiceOptions>>().Value.Registrations);

        // Assert
        Assert.Equal("meu-banco", registration.Name);
        Assert.Equal(TimeSpan.FromSeconds(2), registration.Timeout);
        Assert.Equal(HealthStatus.Degraded, registration.FailureStatus);
        Assert.Contains("ready", registration.Tags);
    }

    [Fact]
    public async Task AddDbContextHealthCheck_WhenExecutedByHealthCheckService_ResolvesScopedContextAndReportsHealthy()
    {
        // Arrange
        var databaseName = Guid.NewGuid().ToString();
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddDbContext<TestRepositoryContext>(options => options.UseInMemoryDatabase(databaseName));
        services.AddHealthChecks().AddDbContextHealthCheck<TestRepositoryContext>();
        using var provider = services.BuildServiceProvider();

        // Act
        var report = await provider.GetRequiredService<HealthCheckService>().CheckHealthAsync();

        // Assert
        Assert.Equal(HealthStatus.Healthy, report.Status);
        Assert.Contains("banco-de-dados", report.Entries.Keys);
    }

    [Fact]
    public void AddDbContextHealthCheck_WhenBuilderIsNull_ThrowsArgumentNullException()
    {
        // Arrange
        IHealthChecksBuilder builder = null!;

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => builder.AddDbContextHealthCheck<TestRepositoryContext>());
    }
}
