using LumiaFoundation.Core.Test.Domain;
using Microsoft.EntityFrameworkCore;

namespace LumiaFoundation.EFRepository.Test.Repository;

public class BaseRepositoryTests
{
    [Fact]
    public void FindAll_WhenTrackChangesIsFalse_ReturnsAllEntitiesWithoutTracking()
    {
        // Arrange
        using var context = CreateContext();
        context.Set<TestEntity>().AddRange(
            new TestEntity { Name = "Alice" },
            new TestEntity { Name = "Bob" });
        context.SaveChanges();
        var repository = new TestRepository(context);

        // Act
        var result = repository.FindAll(trackChanges: false).ToList();

        // Assert
        Assert.Equal(2, result.Count);
        Assert.Equal(["Alice", "Bob"], result.Select(x => x.Name).ToArray());
    }

    [Fact]
    public void FindByCondition_WhenTrackChangesIsTrue_FiltersEntities()
    {
        // Arrange
        using var context = CreateContext();
        context.Set<TestEntity>().AddRange(
            new TestEntity { Name = "Alice" },
            new TestEntity { Name = "Bob" });
        context.SaveChanges();
        var repository = new TestRepository(context);

        // Act
        var result = repository.FindByCondition(entity => entity.Name == "Bob", trackChanges: true).Single();

        // Assert
        Assert.Equal("Bob", result.Name);
    }

    [Fact]
    public void Create_AddsEntityToContext()
    {
        // Arrange
        using var context = CreateContext();
        var repository = new TestRepository(context);
        var entity = new TestEntity { Name = "Alice" };

        // Act
        repository.Create(entity);

        // Assert
        Assert.Contains(entity, context.ChangeTracker.Entries<TestEntity>().Select(entry => entry.Entity));
        Assert.Equal(EntityState.Added, context.Entry(entity).State);
    }

    [Fact]
    public void Update_MarksEntityAsModified()
    {
        // Arrange
        using var context = CreateContext();
        var repository = new TestRepository(context);
        var entity = new TestEntity { Name = "Alice" };

        // Act
        repository.Update(entity);

        // Assert
        Assert.Equal(EntityState.Modified, context.Entry(entity).State);
    }

    [Fact]
    public void Delete_MarksEntityAsDeleted()
    {
        // Arrange
        using var context = CreateContext();
        var repository = new TestRepository(context);
        var entity = new TestEntity { Name = "Alice" };

        // Act
        repository.Delete(entity);

        // Assert
        Assert.Equal(EntityState.Deleted, context.Entry(entity).State);
    }

    private static TestRepositoryContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<TestRepositoryContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new TestRepositoryContext(options);
    }
}
