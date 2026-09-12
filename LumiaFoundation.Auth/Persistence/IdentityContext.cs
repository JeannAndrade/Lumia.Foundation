using LumiaFoundation.Auth.Identity.Model;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

/*
- Esta classe precisa ser herdada no projeto que usar a lib
- O método OnModelCreating precisa ser sobrescrito e chamar o método base.OnModelCreating(modelBuilder)
- Aproveite o método OnModelCreating para configurar as entidades do Identity, como a configuração de roles, usuários e claims.
Ex:
base.OnModelCreating(modelBuilder);
modelBuilder.ApplyConfiguration(new RoleConfiguration());
*/

namespace LumiaFoundation.Auth.Persistence;

public class IdentityContext(DbContextOptions options) : IdentityDbContext<User>(options)

{
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
    }
}