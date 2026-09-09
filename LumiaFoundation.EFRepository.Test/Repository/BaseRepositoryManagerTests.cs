using LumiaFoundation.Core.Test.Domain;
using LumiaFoundation.EFRepository.Repository;
using Microsoft.EntityFrameworkCore;

namespace LumiaFoundation.EFRepository.Test.Repository;

public class BaseRepositoryManagerTests
{
    [Fact]
    public async Task SaveAsync_PersistsPendingChanges()
    {
        // Arrange
        var databaseName = Guid.NewGuid().ToString();
        await using (var context = CreateContext(databaseName))
        {
            var repository = new TestRepository(context);
            repository.Create(new TestEntity { Name = "Alice" });
            var manager = new TestRepositoryManager(context);

            // Act
            await manager.SaveAsync();
        }

        await using var verificationContext = CreateContext(databaseName);

        // Assert
        var persisted = await verificationContext.Set<TestEntity>().SingleAsync();
        Assert.Equal("Alice", persisted.Name);
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
