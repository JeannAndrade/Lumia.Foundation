namespace LumiaFoundation.EFRepository.Repository.IdentityBase
{
    public class IdentityBaseRepositoryManager : IBaseRepositoryManager
    {
        protected readonly IdentityRepositoryContext _repositoryContext;

        public IdentityBaseRepositoryManager(IdentityRepositoryContext repositoryContext)
        {
            _repositoryContext = repositoryContext;
        }

        public async Task SaveAsync() => await _repositoryContext.SaveChangesAsync();
    }
}