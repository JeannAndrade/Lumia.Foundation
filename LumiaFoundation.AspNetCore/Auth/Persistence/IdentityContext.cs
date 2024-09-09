using LumiaFoundation.AspNetCore.Auth.Identity.Model;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

/*
- Esta classe precisa ser herdada no projeto que usar a lib
- Os DbSets precisam ser declarados nesta nova classe
- O método OnModelCreating deve ser sobre escrito para configurar as entidades do projeto
*/


namespace LumiaFoundation.AspNetCore.Auth.Persistence;

public class IdentityContext : IdentityDbContext<User>

{
    public IdentityContext(DbContextOptions options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.ApplyConfiguration(new RoleConfiguration());
    }

    public void InitializeDatabase()
    {
        try
        {
            Console.WriteLine($"Vai executar o Migrate() para o contexto {nameof(IdentityContext)} com a string de conexão: {Database.GetConnectionString()}");

            Database.Migrate();

            Console.WriteLine("Migrate...Ok");
        }
        catch (Exception ex)
        {
            string InnerExceptionMessage = ex.InnerException != null ? ex.InnerException.Message : "";
            Console.WriteLine($"Exception message: {ex.Message}");
            Console.WriteLine($"InnerException message: {InnerExceptionMessage}");
            Console.WriteLine($"StackTrace message: {ex.StackTrace}");

            throw;
        }
    }
}