using LumiaFoundation.EFRepository.Repository;
using LumiaFoundation.Core.Test.Domain;

namespace LumiaFoundation.EFRepository.Test.Repository;

internal sealed class TestRepository(TestRepositoryContext repositoryContext) : BaseRepository<TestEntity>(repositoryContext);
