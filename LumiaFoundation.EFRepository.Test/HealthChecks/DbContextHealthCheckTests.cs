using LumiaFoundation.EFRepository.HealthChecks;
using LumiaFoundation.EFRepository.Test.Repository;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace LumiaFoundation.EFRepository.Test.HealthChecks;

public class DbContextHealthCheckTests
{
    [Fact]
    public async Task CheckHealthAsync_WhenDatabaseIsReachable_ReturnsHealthy()
    {
        // Arrange
        using var context = CreateInMemoryContext();
        var healthCheck = new DbContextHealthCheck<TestRepositoryContext>(context);

        // Act
        var result = await healthCheck.CheckHealthAsync(new HealthCheckContext(), CancellationToken.None);

        // Assert
        Assert.Equal(HealthStatus.Healthy, result.Status);
    }

    [Fact]
    public async Task CheckHealthAsync_WhenDatabaseIsUnreachable_ReturnsUnhealthy()
    {
        // Arrange: a porta 1 em loopback recusa a conexão imediatamente
        var options = new DbContextOptionsBuilder<TestRepositoryContext>()
            .UseNpgsql("Host=127.0.0.1;Port=1;Database=teste;Username=teste;Password=teste;Timeout=1;Pooling=false")
            .Options;
        using var context = new TestRepositoryContext(options);
        var healthCheck = new DbContextHealthCheck<TestRepositoryContext>(context);

        // Act
        var result = await healthCheck.CheckHealthAsync(new HealthCheckContext(), CancellationToken.None);

        // Assert
        Assert.Equal(HealthStatus.Unhealthy, result.Status);
        Assert.Equal("Banco de dados indisponível.", result.Description);
    }

    [Fact]
    public async Task CheckHealthAsync_WhenContextThrows_ReturnsUnhealthyWithoutExposingDetails()
    {
        // Arrange: acessar um contexto descartado lança ObjectDisposedException
        var context = CreateInMemoryContext();
        context.Dispose();
        var healthCheck = new DbContextHealthCheck<TestRepositoryContext>(context);

        // Act
        var result = await healthCheck.CheckHealthAsync(new HealthCheckContext(), CancellationToken.None);

        // Assert
        Assert.Equal(HealthStatus.Unhealthy, result.Status);
        Assert.Equal("Banco de dados indisponível.", result.Description);
        Assert.IsType<ObjectDisposedException>(result.Exception);
    }

    [Fact]
    public async Task CheckHealthAsync_WhenCancellationIsRequested_PropagatesCancellation()
    {
        // Arrange
        using var context = CreateInMemoryContext();
        var healthCheck = new DbContextHealthCheck<TestRepositoryContext>(context);
        using var cancellation = new CancellationTokenSource();
        cancellation.Cancel();

        // Act & Assert
        await Assert.ThrowsAnyAsync<OperationCanceledException>(
            () => healthCheck.CheckHealthAsync(new HealthCheckContext(), cancellation.Token));
    }

    private static TestRepositoryContext CreateInMemoryContext()
    {
        var options = new DbContextOptionsBuilder<TestRepositoryContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new TestRepositoryContext(options);
    }
}
