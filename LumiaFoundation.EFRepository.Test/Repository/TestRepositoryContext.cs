using LumiaFoundation.EFRepository.Repository;
using Microsoft.EntityFrameworkCore;

namespace LumiaFoundation.EFRepository.Test.Repository;

internal sealed class TestRepositoryContext(DbContextOptions options) : RepositoryContext(options)
{
    public DbSet<TestEntity> TestEntities => Set<TestEntity>();
}
