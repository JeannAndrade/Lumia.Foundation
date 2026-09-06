using Microsoft.EntityFrameworkCore;

/*
- Esta classe precisa ser herdada no projeto que usar a lib
- Os DbSets precisam ser declarados nesta nova classe
- O método OnModelCreating deve ser sobre escrito para configurar as entidades do projeto

Algo assim:

namespace Repository.Context;

public class WebapiDbContext(DbContextOptions options) : RepositoryContext(options)
{
  protected override void OnModelCreating(ModelBuilder modelBuilder)
  {
    modelBuilder.ApplyConfigurationsFromAssembly(typeof(WebapiDbContext).Assembly);

    base.OnModelCreating(modelBuilder);
  }

  public DbSet<Company> Companies => Set<Company>();
  public DbSet<Employee> Employees => Set<Employee>();
}

*/


namespace LumiaFoundation.EFRepository.Repository;

public class RepositoryContext(DbContextOptions options) : DbContext(options)
{
  protected override void OnModelCreating(ModelBuilder modelBuilder)
  {
    //modelBuilder.ApplyConfiguration(new SomeEntityConfiguration());
  }

  //public DbSet<SomeEntity> Companies { get; set; }
}