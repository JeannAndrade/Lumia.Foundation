using LumiaFoundation.EFRepository.Identity.Model;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

/*
- Esta classe precisa ser herdada no projeto que usar a lib
- Os DbSets precisam ser declarados nesta nova classe
- O método OnModelCreating deve ser sobre escrito para configurar as entidades do projeto
*/


namespace LumiaFoundation.EFRepository.Repository
{
    public class IdentityRepositoryContext : IdentityDbContext<User>

    {
        public IdentityRepositoryContext(DbContextOptions options) : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
        }
    }
}