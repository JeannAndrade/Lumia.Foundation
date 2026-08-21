namespace LumiaFoundation.EFRepository.Repository
{
    /*
     * BaseRepositoryManager precisa ser herdado de RepositoryManager no projeto cliente.
     * Ele irá fornecer o método SaveAsync() para salvar as alterações no banco de dados.
     */

    public class BaseRepositoryManager(RepositoryContext repositoryContext) : IBaseRepositoryManager
    {
        protected readonly RepositoryContext _repositoryContext = repositoryContext;

        public async Task SaveAsync() => await _repositoryContext.SaveChangesAsync();
    }
}