using Microsoft.EntityFrameworkCore;

/*
- Esta classe precisa ser herdada no projeto que usar a lib
- Os DbSets precisam ser declarados nesta nova classe
- O método OnModelCreating deve ser sobre escrito para configurar as entidades do projeto
*/


namespace LumiaFoundation.Repository
{
  public class RepositoryContext : DbContext
  {
    public RepositoryContext(DbContextOptions options)
    : base(options)
    {
    }
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
      //modelBuilder.ApplyConfiguration(new SomeEntityConfiguration());
    }


    //public DbSet<SomeEntity> Companies { get; set; }
  }

}