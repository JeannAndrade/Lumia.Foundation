namespace JMFoundationLib.RepositoryPattern
{
  public class BaseRepositoryManager : IBaseRepositoryManager
  {
    protected readonly RepositoryContext _repositoryContext;

    public BaseRepositoryManager(RepositoryContext repositoryContext)
    {
      _repositoryContext = repositoryContext;
    }

    public async Task SaveAsync() => await _repositoryContext.SaveChangesAsync();
  }
}