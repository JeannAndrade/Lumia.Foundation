using LumiaFoundation.EFRepository.Domain;

namespace LumiaFoundation.EFRepository.Test.Repository;

internal sealed class TestEntity : Entity
{
    public string Name { get; set; } = string.Empty;
}
