using LumiaFoundation.EFRepository.Repository;

namespace LumiaFoundation.EFRepository.Test.Repository;

internal sealed class TestRepository(TestRepositoryContext repositoryContext) : BaseRepository<TestEntity>(repositoryContext);
