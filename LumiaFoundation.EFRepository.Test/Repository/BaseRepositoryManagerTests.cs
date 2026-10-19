using LumiaFoundation.Core.Test.Domain;
using LumiaFoundation.EFRepository.Repository;
using Microsoft.EntityFrameworkCore;

namespace LumiaFoundation.EFRepository.Test.Repository;

public class BaseRepositoryManagerTests
{
    [Fact]
    public async Task SaveAsync_WithCancellationToken_PersistsPendingChanges()
    {
        // Arrange
        var databaseName = Guid.NewGuid().ToString();
        await using (var context = CreateContext(databaseName))
        {
            var repository = new TestRepository(context);
            repository.Create(new TestEntity { Name = "Bob" });
            var manager = new TestRepositoryManager(context);

            // Act
            await manager.SaveAsync(CancellationToken.None);
        }

        await using var verificationContext = CreateContext(databaseName);

        // Assert
        var persisted = await verificationContext.Set<TestEntity>().SingleAsync();
        Assert.Equal("Bob", persisted.Name);
    }

    [Fact]
    public async Task SaveAsync_WithAlreadyCancelledToken_ThrowsAndDoesNotPersistChanges()
    {
        // Arrange
        var databaseName = Guid.NewGuid().ToString();
        await using (var context = CreateContext(databaseName))
        {
            var repository = new TestRepository(context);
            repository.Create(new TestEntity { Name = "Carol" });
            var manager = new TestRepositoryManager(context);
            using var cancellationTokenSource = new CancellationTokenSource();
            cancellationTokenSource.Cancel();

            // Act / Assert
            await Assert.ThrowsAnyAsync<OperationCanceledException>(
                () => manager.SaveAsync(cancellationTokenSource.Token));
        }

        await using var verificationContext = CreateContext(databaseName);

        // Assert
        Assert.Empty(await verificationContext.Set<TestEntity>().ToListAsync());
    }

    private static TestRepositoryContext CreateContext(string databaseName)
    {
        var options = new DbContextOptionsBuilder<TestRepositoryContext>()
            .UseInMemoryDatabase(databaseName)
            .Options;

        return new TestRepositoryContext(options);
    }

    private sealed class TestRepositoryManager(TestRepositoryContext repositoryContext) : BaseRepositoryManager(repositoryContext);
}
