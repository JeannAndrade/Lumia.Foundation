# Guia de uso do Lumia.Foundation

> **Versão 0.1.0 — 13/11/2026**  
> Primeira versão: visão geral das bibliotecas, criação de um projeto novo e sequência recomendada de implementação.

## Objetivo e escopo

Este guia orienta desenvolvedores que ainda não conhecem o Lumia.Foundation a iniciar um projeto .NET usando as bibliotecas disponíveis. Ele é um ponto de partida, não uma especificação definitiva: deve evoluir conforme novos projetos revelarem padrões, necessidades e melhorias de uso.

Quando uma orientação deste guia divergir do README específico de um pacote, consulte primeiro a documentação do pacote, pois ela contém os detalhes mais recentes de sua API.

## Pré-requisitos

- .NET 10.0 e SDK compatível com o `global.json` da solução.
- Conhecimentos básicos de C#, ASP.NET Core, injeção de dependência e Entity Framework Core.
- MySQL, MariaDB ou PostgreSQL quando o projeto usar persistência ou autenticação baseada em Identity.

## Escolha dos pacotes

| Pacote | Use quando precisar de |
| --- | --- |
| `Lumia.Foundation.Core` | Entidades de domínio, exceções, validação por DataAnnotations, data/hora e paginação. É a base para persistência. |
| `Lumia.Foundation.Abstractions` | Contratos compartilhados de erro (`ErrorDetails`) e resposta paginada (`PagedResponse<T>`). |
| `Lumia.Foundation.Logger` | Logging estruturado com NLog e `ILoggerManager`. |
| `Lumia.Foundation.EFRepository` | `DbContext`, repositórios genéricos, persistência, paginação de consultas e health check do banco. |
| `Lumia.Foundation.AspNetCore.Api` | APIs REST: filtros, controlador base, exceções, health checks, OpenAPI e Scalar. |
| `Lumia.Foundation.Auth` | ASP.NET Core Identity, usuários, JWT e refresh token. |
| `Lumia.Foundation.Http.Client` | Consumo tipado de APIs, tratamento de erros e refresh automático de JWT. |
| `Lumia.Foundation.AspNetCore.Web` | Aplicações MVC/Razor que consomem uma API, autenticação de cliente e paginação por tag helper. |
| `Lumia.Foundation.AspNetCore` | **Legado**. Para projetos novos, prefira os pacotes `.Api` e `.Web`. |

Os pacotes podem ser instalados isoladamente. Uma API normalmente começa com `Core`, `Abstractions`, `Logger`, `EFRepository` e `AspNetCore.Api`; acrescente `Auth` se houver usuários e JWT. Um cliente web normalmente acrescenta `Http.Client` e, se for MVC/Razor, `AspNetCore.Web`.

## Iniciando um projeto

Crie a aplicação com o template apropriado e instale somente as dependências necessárias:

```bash
dotnet new webapi -n Orders.Api
cd Orders.Api

dotnet add package Lumia.Foundation.Core
dotnet add package Lumia.Foundation.Abstractions
dotnet add package Lumia.Foundation.Logger
dotnet add package Lumia.Foundation.EFRepository
dotnet add package Lumia.Foundation.AspNetCore.Api
```

Adicione `Lumia.Foundation.Auth` para autenticação ou `Lumia.Foundation.Http.Client`/`Lumia.Foundation.AspNetCore.Web` para um consumidor de API. Evite usar o pacote `Lumia.Foundation.AspNetCore` em código novo, pois ele foi substituído pelos pacotes especializados.

Uma organização inicial possível é:

```text
Orders.Api/
  Domain/             entidades e regras de domínio
  Application/        casos de uso, comandos, DTOs e interfaces
  Persistence/        DbContext, configurações e repositórios
  Controllers/        endpoints HTTP
  Program.cs
  appsettings.json
```

Mantenha o domínio sem dependência de ASP.NET Core. Use `Core` nas camadas internas e deixe conceitos HTTP (status code, controllers e handlers) na borda da aplicação.

## Sequência recomendada de construção

### 1. Defina o domínio

Comece pelas entidades e regras de negócio. Entidades persistidas devem herdar de `Entity`:

```csharp
using LumiaFoundation.Core.Domain;

public sealed class Order : Entity
{
    public DateTime CreatedAt { get; set; }
}
```

Use `DomainBaseException`, `EntityNotFoundException`, `EntityInUseException` e `CommandValidationException` para expressar regras sem acoplar o domínio a HTTP. Para validar comandos com DataAnnotations:

```csharp
CommandValidator.Validate(command);
```

Quando a aplicação precisar de data/hora substituível em testes, injete `IDateTimeProvider` e use `UtcDateTimeProvider` ou `LocalDateTimeProvider` conforme a regra do projeto.

### 2. Modele a persistência

Crie o contexto herdando de `RepositoryContext`, declare os `DbSet`s e aplique as configurações:

```csharp
public sealed class AppDbContext(DbContextOptions<AppDbContext> options)
    : RepositoryContext(options)
{
    public DbSet<Order> Orders => Set<Order>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
    }
}
```

Registre o contexto com `ConfigureMySqlDbDatabase<T>()` ou `ConfigurePostgreSqlDatabase<T>()`, usando `DbConnectionHelper`. Obtenha credenciais de configuração, variáveis de ambiente ou secret manager; não as coloque no código-fonte.

Para consultas específicas, herde de `BaseRepository<T>`. Use `trackChanges: false` em leituras. Agrupe a persistência em um `BaseRepositoryManager` e chame `SaveAsync(cancellationToken)` após as alterações.

Para listas, aplique `ToPagedListAsync(page, pageSize, cancellationToken)` e devolva `PagedResponse<T>` na fronteira HTTP.

### 3. Configure logging primeiro no host

Instale `Lumia.Foundation.Logger`. O pacote copia um `nlog.config` inicial sem substituir um arquivo já customizado. No `Program.cs`:

```csharp
LoggerManager.LoadConfigurationFromFile(
    Path.Combine(builder.Environment.ContentRootPath, "nlog.config"));
builder.Services.ConfigureLoggerService();
```

Injete `ILoggerManager` nos serviços. Prefira mensagens com placeholders nomeados e propriedades estruturadas, sem registrar senhas, tokens ou dados pessoais desnecessários.

### 4. Registre os recursos da API

No `Program.cs`, registre serviços por assembly, filtros e handlers:

```csharp
builder.Services.AddServicesFromAssembly(typeof(Program).Assembly);
builder.Services.AddValidationFilters();
builder.Services.AddExceptionHandlers();
```

Use `RequestValidationAttribute` para validar corpos e `ModelState`, ou `DtoNotEmptyValidationAttribute` quando esse for o comportamento desejado e o filtro estiver registrado no DI. Para controllers, `BaseApiController` traduz exceções de domínio comuns para respostas HTTP padronizadas.

Complete o pipeline:

```csharp
var app = builder.Build();
app.UseExceptionHandler();
app.MapControllers();
app.MapLumiaHealthChecks();
app.Run();
```

Configure também OpenAPI/Scalar com `ConfigureOpenApi` e `MapOpenApiDevTools` durante o desenvolvimento.

### 5. Adicione autenticação somente quando necessário

Com `Lumia.Foundation.Auth`, crie seu contexto de Identity herdando de `IdentityContext`, chame `base.OnModelCreating(modelBuilder)` e aplique as configurações de roles. Depois, configure Identity, banco, parâmetros JWT e o `ServiceManager`:

```csharp
var appConfig = new AppConfigurationParameter(builder.Configuration);
builder.Services.ConfigureIdentity();
builder.Services.ConfigureAppSettingsReader(appConfig);
builder.Services.ConfigureJWT(appConfig);
builder.Services.ConfigureIdentityServiceManager();
```

Defina `JWT_SECRET` fora do repositório. A autenticação deve ser integrada depois que o fluxo básico da API, persistência e tratamento de erros estiverem funcionando.

### 6. Construa consumidores de API

Em um projeto que consome a API, registre `AddLumiaApiClient` e forneça uma implementação de `ITokenStore` adequada ao tipo de host. `InMemoryTokenStore` é apropriado para testes, scripts e processos únicos; aplicações multiusuário precisam de armazenamento associado ao usuário.

```csharp
builder.Services.AddLumiaApiClient("https://api.example.com");
builder.Services.AddApiResourceClient<IOrderApi, OrderApi>();
```

Use `IApiConnection` nos clientes de recurso. Respostas HTTP malsucedidas geram `ApiException`; trate o `StatusCode` sem duplicar a lógica de serialização de erros.

## Convenções de uso

- Propague `CancellationToken` em consultas, chamadas HTTP e `SaveAsync`.
- Não exponha diretamente entidades de persistência: use DTOs na API.
- Padronize erros com `ErrorDetails` e mantenha mensagens apropriadas para o cliente.
- Use health checks de liveness e readiness; marque a conexão do banco com a tag usada pelo readiness.
- Escreva testes unitários para domínio e serviços e testes de integração para persistência, pipeline HTTP e autenticação.
- Consulte os READMEs dos pacotes para exemplos completos: `LumiaFoundation.Core/docs/readme.md`, `LumiaFoundation.EFRepository/docs/readme.md` e os demais diretórios `docs`.

## Checklist de um primeiro incremento

- [ ] Projeto criado e target .NET confirmado.
- [ ] Pacotes mínimos instalados.
- [ ] Entidade e regra de domínio implementadas.
- [ ] Contexto, configurações EF e migration criados.
- [ ] Repositório e caso de uso testados.
- [ ] Logger e tratamento global de exceções configurados.
- [ ] Endpoint com DTO, validação e paginação, se aplicável.
- [ ] Health checks, OpenAPI e autenticação configurados conforme necessidade.
- [ ] Testes executados com `dotnet test LumiaFoundation.slnx`.

## Evolução deste guia

Este documento será ampliado iterativamente a partir da experiência dos projetos que usam o Lumia.Foundation. Ao descobrir uma convenção, corrigir uma sequência ou adicionar um cenário recorrente, atualize o conteúdo e registre a mudança abaixo. Alterações incompatíveis ou mudanças de orientação devem receber uma nova versão claramente descrita.

## Histórico de versões

### 0.1.0 — 13/11/2026

- Criado o guia inicial para desenvolvedores iniciantes.
- Documentados os pacotes, suas responsabilidades e a combinação recomendada.
- Adicionada uma sequência de construção de domínio, persistência, infraestrutura, API, autenticação e clientes.
- Adicionados exemplos mínimos de instalação, configuração e checklist.
- Registrada a expectativa de evolução do documento conforme novos projetos forem desenvolvidos.
