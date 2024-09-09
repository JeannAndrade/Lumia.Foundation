namespace LumiaFoundation.EFRepository.Domain
{
  public abstract class Entity
  {
    public Guid Id { get; private set; }

    public Entity()
    {
      Id = Guid.NewGuid();
    }
  }
}