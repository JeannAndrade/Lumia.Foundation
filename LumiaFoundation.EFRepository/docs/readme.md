# Lumia.Foundation.EFRepository

Biblioteca com classes base para projetos que usam Entity Framework Core, incluindo contexto, repositórios genéricos e configuração de banco de dados MySQL/MariaDB/PostgreSQL.

## Instalação

Instale o pacote no projeto que contém a aplicação ou a camada de persistência:

```bash
dotnet add package Lumia.Foundation.EFRepository
```

O pacote atualmente utiliza Entity Framework Core 10 e oferece suporte aos provedores MySQL, MariaDB e PostgreSQL incluídos como dependências.

## Como usar

### 1. Crie o contexto da aplicação

O `RepositoryContext` deve ser herdado no projeto consumidor. Declare nele os `DbSet`s da aplicação e aplique as configurações das entidades:

```csharp
using LumiaFoundation.EFRepository.Repository;
using Microsoft.EntityFrameworkCore;

namespace MyApp.Persistence;

public class AppDbContext(DbContextOptions<AppDbContext> options)
 : RepositoryContext(options)
{
 protected override void OnModelCreating(ModelBuilder modelBuilder)
 {
  modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
  base.OnModelCreating(modelBuilder);
 }

 public DbSet<Order> Orders => Set<Order>();
}
```

### 2. Configure a conexão

Crie um `DbConnectionHelper` com os dados da conexão e registre o contexto na injeção de dependência. O método recomendado para MySQL é `ConfigureMySqlDbDatabase<T>()`:

```csharp
using LumiaFoundation.EFRepository.Extensions;
using LumiaFoundation.EFRepository.Utils;

var connection = new DbConnectionHelper(
 host: "localhost",
 port: "3306",
 user: "app_user",
 password: "app_password",
 database: "my_app",
 majorVersion: 8,
 minorVersion: 0,
 buildVersion: 0);

builder.Services.ConfigureMySqlDbDatabase<AppDbContext>(
 connection,
 migrationsAssembly: typeof(AppDbContext).Assembly.GetName().Name!);
```

Para PostgreSQL, use `ConfigurePostgreSqlDatabase<T>()` com o mesmo `DbConnectionHelper`:

```csharp
builder.Services.ConfigurePostgreSqlDatabase<AppDbContext>(
 connection,
 migrationsAssembly: typeof(AppDbContext).Assembly.GetName().Name!);
```

Para MariaDB, a versão `0.7.0` ainda disponibiliza `ConfigureMariaDbDatabase<T>()`, mas esse método está obsoleto. Prefira o método recomendado pelo provedor adotado pelo projeto antes de iniciar uma nova integração.

> Evite manter usuário e senha diretamente no código. Em uma aplicação real, obtenha esses valores de configuração, variáveis de ambiente ou um gerenciador de segredos.

### 3. Crie as entidades e os repositórios

As entidades usadas com `BaseRepository<T>` devem herdar de `Entity`:

```csharp
using LumiaFoundation.EFRepository.Domain;

public class Order : Entity
{
 public DateTime CreatedAt { get; set; }
}
```

Para criar um repositório específico, herde de `BaseRepository<T>`:

```csharp
using LumiaFoundation.EFRepository.Repository;

public class OrderRepository(AppDbContext context)
 : BaseRepository<Order>(context)
{
}
```

O repositório base oferece:

- `FindAll(bool trackChanges)` para consultar todas as entidades.
- `FindByCondition(...)` para consultar por uma expressão.
- `Create`, `Update` e `Delete` para alterar o estado das entidades.

Use `trackChanges: false` em consultas somente leitura para evitar o rastreamento desnecessário pelo Entity Framework Core.

### 4. Salve as alterações

Herde de `BaseRepositoryManager` no projeto consumidor para expor `SaveAsync()`:

```csharp
using LumiaFoundation.EFRepository.Repository;

public class RepositoryManager(AppDbContext context)
 : BaseRepositoryManager(context)
{
}
```

Depois de adicionar, atualizar ou remover entidades, chame `SaveAsync()` para persistir as alterações:

```csharp
orderRepository.Create(order);
await repositoryManager.SaveAsync();
```

## Componentes disponíveis

| Componente | Finalidade |
| :--- | :--- |
| `RepositoryContext` | Classe base para o `DbContext` da aplicação. |
| `BaseRepository<T>` | Operações genéricas de consulta e alteração para entidades. |
| `BaseRepositoryManager` | Persistência das alterações com `SaveAsync()`. |
| `DbConnectionHelper` | Dados da conexão e versão do servidor para configurar o provedor. |
| `ConfigureMySqlDbDatabase<T>()` | Registro do contexto usando o provedor MySQL. |
| `ConfigureMariaDbDatabase<T>()` | Registro do contexto usando o provedor MariaDB. |
| `ConfigurePostgreSqlDatabase<T>()` | Registro do contexto usando o provedor PostgreSQL. |

## Histórico de versões

### 0.8.1

- Adicionado suporte ao provedor Oracle MySQL com `MySql.EntityFrameworkCore` (10.0.9).
- Atualizado o provedor MariaDB `Microting.EntityFrameworkCore.MySql` para a versão 10.0.11.
- Removida a obsolescência do método `ConfigureMariaDbDatabase<T>()`.

### 0.8.0

- Adicionado suporte ao pacote `Npgsql.EntityFrameworkCore.PostgreSQL` (10.0.3).
- Adicionado o método `ConfigurePostgreSqlDatabase<T>()` para configuração de banco PostgreSQL.
- Atualizando o pacote `Microting.EntityFrameworkCore.MySql` (10.0.11).

### 0.7.0

- Suporte ao pacote `MySql.EntityFrameworkCore`.
- Adicionado o pacote `Microting.EntityFrameworkCore.MySql` (10.0.10).
- Adicionado o método `ConfigureMySqlDbDatabase<T>()` para configuração de banco MySQL.
- O método `ConfigureMariaDbDatabase<T>()` foi marcado como obsoleto.
- Atualizado para Entity Framework Core 10.0.11.

### 0.2.0

- Adicionado suporte ao banco MariaDB.
