namespace LumiaFoundation.EFRepository.Repository
{
  /*
     * IBaseRepositoryManager precisa ser herdado de IRepositoryManager no projeto cliente.
     * Ele irá fornecer o contrato para o método SaveAsync() para salvar as alterações no banco de dados.
     */
  public interface IBaseRepositoryManager
  {
    Task SaveAsync();
  }

}