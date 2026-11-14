# Guia de uso do Lumia.Foundation

> **Versão 0.2.0 — 13/11/2026**
> Acrescenta exemplos reais extraídos do projeto Vault, a organização de camadas efetivamente adotada, lições aprendidas (armadilhas) e um checklist ampliado.

## Objetivo e escopo

Este guia orienta desenvolvedores que ainda não conhecem o Lumia.Foundation a iniciar um projeto .NET usando as bibliotecas disponíveis. Ele é um ponto de partida, não uma especificação definitiva: deve evoluir conforme novos projetos revelarem padrões, necessidades e melhorias de uso.

Quando uma orientação deste guia divergir do README específico de um pacote, consulte primeiro a documentação do pacote, pois ela contém os detalhes mais recentes de sua API.

### Como ler os exemplos do Vault

O **Vault** é um gerenciador de investimentos pessoais multiusuário (renda fixa e ações) construído sobre o Lumia.Foundation. Ao longo do guia, os blocos marcados com **Exemplo no Vault** mostram código real (às vezes resumido), com o caminho do arquivo de origem. Os blocos marcados com **Armadilha** descrevem problemas que o projeto de fato encontrou ou que ficaram visíveis no código, para que o próximo projeto não os repita.

Vocabulário do domínio do Vault usado nos exemplos:

| Termo | Significado |
| --- | --- |
| `Movimento` | Um aporte/investimento (valor, datas, vencimento, valor líquido atual) |
| `Objetivo` | Meta financeira (valor da meta, aporte mensal planejado) |
| `Corretora`, `Emissor`, `Produto`, `TipoRenda` | Cadastros de apoio, referenciados por todo `Movimento` |
| `Cotacao` | Cotação diária de um papel, usada para atualizar movimentos |
| `OwnerId` | Identificador do usuário dono do registro (base do multi-tenant) |

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

> **Exemplo no Vault — pacote por camada.** Cada projeto referencia apenas o que a sua camada precisa:
>
> | Projeto | Pacotes Lumia | Observação |
> | --- | --- | --- |
> | `Application` | `Lumia.Foundation.Core` 0.13.1 | Nenhum pacote de EF Core, ASP.NET Core ou logger |
> | `Persistence` | `Lumia.Foundation.EFRepository` 0.14.1, `Lumia.Foundation.Auth` 0.3.1 | Mais `Microsoft.EntityFrameworkCore.Design` com `PrivateAssets=all` |
>
> `Application.csproj` referencia só `Domain`; `Persistence.csproj` referencia `Application` e `Domain`. Isso torna impossível, por construção, a Application depender de infraestrutura.

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

Uma organização inicial possível (projeto único) é:

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

> **Exemplo no Vault — uma solução com um projeto por camada.** Quando o projeto cresce, cada camada vira um projeto, e as dependências apontam para dentro:
>
> ```text
> Web  ──HTTP──▶  Api  ──▶  Service (raiz de composição)
>                             ├──▶ Application ──▶ Domain
>                             └──▶ Persistence ──▶ Application, Domain
> ```
>
> Regras que sustentam esse desenho (documentadas nos READMEs de `Application` e `Persistence`):
>
> - A **Application** conhece só o `Domain` e o pacote `Core`. Ela declara as *portas* (`IRepositoryManager`, `I<Entidade>Repository`) e a **Persistence** as implementa (inversão de dependência).
> - O **Service** é o único que enxerga as duas pontas e registra tudo no contêiner.
> - O **Web** nunca acessa o banco: só a Api, por contrato HTTP.
> - Um teste de arquitetura (`Service.Test/Arquitetura/DependenciasEntreCamadasTests`) falha se alguém inverter uma dependência. Vale copiar a ideia em todo projeto com mais de uma camada.
>
> Estrutura interna da Application: `<Agregado>/Commands/<Caso>`, `<Agregado>/Queries/<Caso>`, `Repositories/`, `Pagination/`. Cada caso de uso é um par `I<Caso>` + `<Caso>` (por exemplo `ICreateCorretoraCommand` e `CreateCorretoraCommand`).

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

> **Exemplo no Vault — as quatro exceções em um caso de uso real.** `DeleteTipoRendaCommand` usa duas delas, sem mencionar HTTP:
>
> ```csharp
> // Application/TiposRenda/Commands/DeleteTipoRenda/DeleteTipoRendaCommand.cs
> public class DeleteTipoRendaCommand(IRepositoryManager repositoryManager) : IDeleteTipoRendaCommand
> {
>     private readonly IRepositoryManager _repositoryManager = repositoryManager;
>
>     public async Task ExecuteAsync(Guid ownerId, Guid tipoRendaId, CancellationToken cancellationToken)
>     {
>         var tipoRenda = await _repositoryManager.TipoRenda.GetAsync(ownerId, tipoRendaId, trackChanges: true, cancellationToken)
>             ?? throw new EntityNotFoundException("Tipo de renda não encontrado");
>
>         if (await _repositoryManager.Movimento.ExistemPorTipoRendaAsync(tipoRendaId, cancellationToken))
>             throw new EntityInUseException("Não é possível excluir o Tipo de Renda pois existem Movimentos associados a ele.");
>
>         _repositoryManager.TipoRenda.Delete(tipoRenda);
>         await _repositoryManager.SaveAsync(cancellationToken);
>     }
> }
> ```
>
> Pontos a observar: o `ownerId` entra na busca (um ID de outro usuário se comporta como inexistente); a regra "não excluir se em uso" é uma exceção de domínio; o `CancellationToken` chega até o `SaveAsync`.

> **Exemplo no Vault — receita de um caso de uso.** Todo comando segue a mesma ordem: validar, buscar com escopo de dono, aplicar a regra, persistir, devolver um modelo.
>
> ```csharp
> // Application/Corretoras/Commands/CreateCorretora/CreateCorretoraCommand.cs
> public async Task<CorretoraModel> ExecuteAsync(
>     CorretoraModelForCreation corretoraModel, Guid ownerId, CancellationToken cancellationToken)
> {
>     CommandValidator.Validate(corretoraModel);               // 1. validação primeiro (erro do cliente = 422)
>
>     var corretora = corretoraModel.ToDomain(ownerId);        // 2. o dono vem do chamador autenticado
>     _repositoryManager.Corretora.Create(corretora);
>     await _repositoryManager.SaveAsync(cancellationToken);   // 3. persiste com cancelamento
>
>     return CorretoraModel.FromDomain(corretora);             // 4. devolve DTO, nunca a entidade
> }
> ```

> **Exemplo no Vault — o trio de modelos por agregado.** Para cada agregado existem `<X>Model` (saída, com `FromDomain`), `<X>ModelForCreation` (entrada, com `ToDomain`) e `<X>ModelForUpdate` (entrada, com `UpdateDomain`). Mensagens de validação ficam em português, junto dos atributos:
>
> ```csharp
> // Application/Corretoras/Commands/UpdateCorretora/CorretoraModelForUpdate.cs
> public record CorretoraModelForUpdate
> {
>     [Required(ErrorMessage = "Nome da Corretora é obrigatório")]
>     [MaxLength(60, ErrorMessage = "Nome da Corretora não deve exceder 60 caracteres")]
>     public string Nome { get; set; } = string.Empty;
>
>     public Corretora UpdateDomain(Corretora corretora) { corretora.Nome = Nome; return corretora; }
> }
> ```

> **Exemplo no Vault — regras que cruzam campos.** DataAnnotations não expressam "se `AtualizaPorCotacao` for verdadeiro, `Quantidade` é obrigatória". Para isso o modelo implementa `IValidatableObject`, e o `CommandValidator.Validate` continua sendo a única porta de entrada:
>
> ```csharp
> // Application/Movimentos/Commands/CreateMovimento/MovimentoModelForCreation.cs
> public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
> {
>     if (AtualizaPorCotacao && (Quantidade is null || Quantidade <= 0))
>         yield return new ValidationResult(
>             "Quantidade é obrigatória e deve ser maior que zero quando o valor varia por cotação",
>             new[] { nameof(Quantidade) });
> }
> ```
>
> Para identificadores obrigatórios, o Vault usa `[NotEmptyGuid]` (de `LumiaFoundation.Core.ValidationAttributes`) em vez de `[Required]`, porque `[Required]` nunca falha em um `Guid` (o valor padrão `Guid.Empty` é considerado "presente").

> **Exemplo no Vault — regras de negócio dentro da entidade.** Em vez de espalhar `if` nos comandos, o domínio expõe operações com nome de negócio: `movimento.Encerrar(motivo, data, hoje)`, `movimento.Reativar()` e `movimento.AplicarCotacao(valor, hoje)`. Os comandos só orquestram:
>
> ```csharp
> // Application/Movimentos/Commands/EncerrarMovimento/EncerrarMovimentoCommand.cs (trecho)
> var hoje = _dateTimeProvider.GetDateTime().Date;
> EncerramentoValidador.Validar(movimento.DataInvestimento, model.Data!.Value, hoje);
> movimento.Encerrar(model.Motivo!.Value, model.Data.Value, hoje);
> ```
>
> Cálculos puros (rentabilidade, distribuição de valor líquido, projeção de objetivos, janelas mensais) ficam em classes estáticas do `Domain` (`RentabilidadeCalculador`, `DistribuicaoValorLiquidoCalculador`, `ProjecaoObjetivoCalculador`, `JanelaMensal`). São triviais de testar: entram valores, saem valores, sem banco e sem relógio.

> **Exemplo no Vault — data/hora injetável.** Nenhum caso de uso chama `DateTime.Now`. O "hoje" vem sempre do provedor, o que permite testar regras como "a data de encerramento não pode ser futura":
>
> ```csharp
> // Application/Movimentos/Commands/UpdateValorLiquidoMovimento/UpdateValorLiquidoMovimentoCommand.cs
> model.UpdateDomain(movimento, _dateTimeProvider.GetDateTime().Date);
> ```

> **Armadilha — dono do registro vindo do corpo da requisição.** Dois estilos convivem no Vault:
>
> - `CorretoraModelForCreation.ToDomain(Guid ownerId)` e `MovimentoModelForCreation.ToDomain(ownerId, ...)`: o dono é um parâmetro do caso de uso, preenchido a partir do usuário autenticado. **Este é o padrão recomendado.**
> - `TipoRendaModelForCreation(string Nome, Guid OwnerId)`, `ProdutoModelForCreation`, `EmissorModelForCreation` e `ObjetivoModelForCreation`: o `OwnerId` é um campo do próprio modelo. Se a Api desserializar o corpo da requisição direto nesse tipo sem sobrescrever o campo, um cliente malicioso poderia criar registros em nome de outro usuário. Além disso, `[Required]` em `Guid` não bloqueia `Guid.Empty`.
>
> Em projetos multiusuário, o dono **nunca** deve ser lido do corpo da requisição. Recomenda-se migrar todos os `ModelForCreation` para o padrão `ToDomain(ownerId)` e cobrir com teste de integração que tente "forjar" outro `OwnerId`.

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

> **Exemplo no Vault — o contexto.** Dois contextos separados: `VaultDbContext` (domínio) e `VaultIdentityDbContext` (Identity), cada um com suas migrations (`Migrations/VaultDb` e `Migrations/VaultIdentityDb`). O contexto de domínio carrega todos os mapeamentos do assembly, exceto o de roles, que pertence ao contexto de Identity:
>
> ```csharp
> // Persistence/Context/VaultDbContext.cs
> protected override void OnModelCreating(ModelBuilder modelBuilder)
> {
>     modelBuilder.ApplyConfigurationsFromAssembly(
>         typeof(VaultDbContext).Assembly,
>         type => type != typeof(RoleConfiguration));
>
>     base.OnModelCreating(modelBuilder);
> }
> ```
>
> ```csharp
> // Persistence/Context/VaultIdentityDbContext.cs
> public class VaultIdentityDbContext(DbContextOptions<VaultIdentityDbContext> options) : IdentityContext(options)
> {
>     protected override void OnModelCreating(ModelBuilder modelBuilder)
>     {
>         base.OnModelCreating(modelBuilder);
>         modelBuilder.ApplyConfiguration(new RoleConfiguration());
>     }
> }
> ```

> **Exemplo no Vault — o mapeamento (`IEntityTypeConfiguration<T>`).** Um arquivo por entidade em `Persistence/Mappings/`. Repare em: nomes de coluna explícitos, tamanhos, precisão de decimais, enum gravado como texto e índices compostos sempre começando por `OwnerId`:
>
> ```csharp
> // Persistence/Mappings/MovimentoMapping.cs (trechos)
> builder.ToTable("Movimentos", table =>
>     table.HasCheckConstraint("CK_Movimentos_Encerramento",
>         "((EstaAtivo = 1 AND MotivoEncerramento IS NULL AND DataEncerramento IS NULL) OR " +
>         "(EstaAtivo = 0 AND MotivoEncerramento IS NOT NULL AND DataEncerramento IS NOT NULL))"));
>
> builder.Property(e => e.ValorAporte).IsRequired().HasPrecision(18, 2);
> builder.Property(e => e.MotivoEncerramento).HasConversion<string>().HasMaxLength(20);
> builder.Property(e => e.DataEncerramento).HasColumnType("date");
>
> // Índices compostos — sempre começando por OwnerId
> builder.HasIndex(e => new { e.OwnerId, e.DataInvestimento });
> builder.HasIndex(e => new { e.OwnerId, e.EstaAtivo, e.DataVencimento });
>
> builder.HasOne(e => e.Objetivo).WithMany(c => c.Movimentos)
>     .HasForeignKey(e => e.ObjetivoId).OnDelete(DeleteBehavior.Restrict);
> ```
>
> Três decisões vale replicar:
>
> 1. **Invariante também no banco.** A regra "movimento ativo não tem motivo/data de encerramento; encerrado tem os dois" existe na entidade (`Encerrar`/`Reativar`), no validador e como `CHECK` no MariaDB. Se um bug de código tentar gravar um estado inválido, o banco recusa.
> 2. **Enum como texto exige contrato.** Como `MotivoEncerramento` é gravado como string, o nome dos valores do enum virou contrato de persistência; renomear exige migration de dados (registrado no ADR 0020).
> 3. **Índice acoplado à consulta.** Os comentários do mapeamento apontam qual consulta e qual ADR sustentam cada índice (por exemplo, a listagem paginada ordena por `DataInvestimento DESC, Id DESC`). Se alguém mudar a ordenação, o comentário avisa que o índice deixa de ajudar.

> **Exemplo no Vault — repositórios e o gerenciador.** Cada repositório estende `BaseRepository<T>` e usa `FindByCondition(predicado, trackChanges)`. O `RepositoryManager` cria os repositórios sob demanda (`Lazy`) sobre o **mesmo** `DbContext`, de modo que um único `SaveAsync` confirma tudo em uma unidade de trabalho:
>
> ```csharp
> // Persistence/Corretoras/CorretoraRepository.cs
> public async Task<Corretora?> GetAsync(Guid ownerId, Guid corretoraId, bool trackChanges, CancellationToken cancellationToken) =>
>     await FindByCondition(c => c.OwnerId == ownerId && c.Id == corretoraId, trackChanges)
>         .SingleOrDefaultAsync(cancellationToken);
> ```
>
> ```csharp
> // Persistence/Management/RepositoryManager.cs (trecho)
> public class RepositoryManager(VaultDbContext repositoryContext) : BaseRepositoryManager(repositoryContext), IRepositoryManager
> {
>     private readonly Lazy<ICorretoraRepository> _corretoraRepository = new(() => new CorretoraRepository(repositoryContext));
>     // ... um Lazy por repositório
>     public ICorretoraRepository Corretora => _corretoraRepository.Value;
> }
> ```
>
> Consequência: os repositórios **não** são registrados no contêiner; apenas `IRepositoryManager`. Os casos de uso só recebem `IRepositoryManager`, nunca repositórios isolados. Isso evita que dois repositórios usem contextos diferentes sem querer.

> **Exemplo no Vault — multi-tenant na camada de dados.** Toda consulta recebe `ownerId` e filtra por ele. O ID de outro usuário simplesmente "não existe". Até a validação de referências usa esse filtro: antes de criar um `Movimento`, `MovimentoReferenciasValidator` consulta Objetivo, TipoRenda, Corretora, Produto e Emissor **do mesmo dono**; se qualquer um não pertencer ao usuário, a primeira falha interrompe com `EntityNotFoundException`.
>
> Teste obrigatório para qualquer consulta nova: criar dados para dois `OwnerId` e provar que o segundo nunca enxerga o primeiro.

> **Exemplo no Vault — paginação ordenada e determinística.**
>
> ```csharp
> // Persistence/Movimentos/MovimentoRepository.cs (trecho)
> return await query
>     .OrderByDescending(m => m.DataInvestimento)
>     .ThenByDescending(m => m.Id)              // desempate determinístico
>     .ToPagedListAsync(page, pageSize, cancellationToken);
> ```
>
> A Application valida os parâmetros com DataAnnotations (`PaginationParameters`: página ≥ 1, tamanho entre 1 e 100) e converte o resultado com `movimentos.Map(MovimentoModel.FromDomain)`, preservando os metadados de paginação.
>
> ```csharp
> // Application/Pagination/PaginationParameters.cs
> public record PaginationParameters
> {
>     public const int PageSizeDefault = 20;
>     public const int PageSizeMaximum = 100;
>
>     [Range(1, int.MaxValue, ErrorMessage = "A página deve ser maior ou igual a {1}")]
>     public int Page { get; init; } = 1;
>
>     [Range(1, PageSizeMaximum, ErrorMessage = "O tamanho da página deve estar entre {1} e {2}")]
>     public int PageSize { get; init; } = PageSizeDefault;
> }
> ```

> **Exemplo no Vault — registro e configuração do banco.** Um único método de extensão registra a persistência; as variáveis de ambiente (`DBHOST`, `DBPORT`, `VAULT_DB_USER`, `VAULT_DB_PASS`, `VAULT_DB_NAME`, `DBMAJORVERSION`...) alimentam o `DbConnectionHelper`. O Vault usa `ConfigureMariaDbDatabase<TContext>` para MariaDB:
>
> ```csharp
> // Persistence/Extensions/ServiceExtensions.cs (trecho)
> public static IServiceCollection AddPersistence(this IServiceCollection services, IConfiguration configuration)
> {
>     services.ConfigureDatabase(configuration);
>     services.ConfigureIdentityDatabase(configuration);
>     services.AddScoped<IRepositoryManager, RepositoryManager>();
>     return services;
> }
> ```
>
> O registro por convenção (`AddServicesFromAssembly`) não alcança essas classes, porque a interface fica na Application, em outro assembly. Por isso o registro explícito de `IRepositoryManager`.

> **Armadilha — decimais sem precisão.** Duas migrations do Vault (`ObjetivoMetaPrecisao` e `ObjetivoAporteMensalPrecisao`) existem só para corrigir colunas `decimal(65,30)`, o padrão gerado pelo provedor MySQL/MariaDB quando não se declara precisão. Sempre use `HasPrecision(18, 2)` (ou a precisão do domínio) em todo `decimal` monetário **na primeira migration**. As migrations de correção também mostram o cuidado de validar a faixa nos modelos (`Meta` até 16 dígitos inteiros e 2 casas decimais, via `IValidatableObject`).

> **Armadilha — seed (`HasData`) com valores aleatórios.** `IdentityRole` gera `Id` e `ConcurrencyStamp` aleatórios a cada instância. Resultado no histórico do Vault: **quase toda migration de `VaultDb` entre `TabelasDomain` e `IndiceMovimentosProximosVencimentos` contém um `DeleteData` + `InsertData` de `IdentityRole`** que nada tem a ver com a mudança real. No contexto de Identity isso seria pior: apagar a role removeria, por cascata (`AspNetUserRoles`), o vínculo dela com os usuários. A correção foi dupla: (1) `RoleConfiguration` com valores fixos e um comentário "NÃO os altere"; (2) remoção do `IdentityRole` do contexto de domínio (migration `RemocaoIdentityRoleDoDominio`).
>
> ```csharp
> // Persistence/Context/RoleConfiguration.cs
> public const string AdministratorId = "a6555a52-0a06-44c6-977b-3183faa171d9";
> public const string AdministratorConcurrencyStamp = "7729cb8f-9cd8-4292-8f0d-67810bd24e5c";
> ```
>
> Regra prática: dados de seed precisam de chaves **determinísticas**; e cada tabela deve pertencer a um único contexto.

> **Armadilha — renomear propriedade sem renomear coluna.** Na migration `RenameUserParaOwner`, a propriedade passou a se chamar `OwnerId`, mas nas demais tabelas a coluna continuou `UserId` (via `HasColumnName("UserId")`), enquanto em `Cotacoes` a coluna foi realmente renomeada. O resultado funciona, mas há duas convenções no mesmo schema (o nome do índice `IX_Movimentos_UserId_EstaAtivo_DataAtualizacaoValorLiquido` também ficou com `UserId` via `HasDatabaseName`). Decida cedo se o nome físico acompanha o nome do código e registre a decisão em ADR; qualquer mudança posterior de schema exige discussão prévia.

> **Exemplo no Vault — migrations e fábricas de design-time.** Como há dois contextos, existem duas `IDesignTimeDbContextFactory`, que montam um `ServiceCollection` mínimo com as mesmas extensões de produção e leem a configuração das variáveis de ambiente:
>
> ```csharp
> // Persistence/ContextFactory/PersistenceContextFactory.cs
> public VaultDbContext CreateDbContext(string[] args)
> {
>     var configuration = new ConfigurationBuilder()
>         .SetBasePath(Directory.GetCurrentDirectory())
>         .AddEnvironmentVariables()
>         .Build();
>
>     var services = new ServiceCollection();
>     services.ConfigureDatabase(configuration);
>     return services.BuildServiceProvider().GetRequiredService<VaultDbContext>();
> }
> ```
>
> Gerar migrations de cada contexto (a pasta de saída separa os dois históricos):
>
> ```bash
> dotnet ef migrations add NomeDaMigration \
>   --project Persistence --context VaultDbContext --output-dir Migrations/VaultDb
>
> dotnet ef migrations add NomeDaMigration \
>   --project Persistence --context VaultIdentityDbContext --output-dir Migrations/VaultIdentityDb
> ```
>
> A aplicação das migrations em ambientes é feita por *migration bundle* (ADR 0001), não pela API em tempo de execução.

> **Exemplo no Vault — migration que também migra dados.** Ao introduzir `MotivoEncerramento`/`DataEncerramento` com a restrição `CHECK`, os registros já inativos precisavam ser ajustados **antes** de a restrição existir. A migration faz isso com SQL explícito, na ordem certa: colunas, `UPDATE` dos dados legados, depois `AddCheckConstraint`.

### 3. Configure logging primeiro no host

Instale `Lumia.Foundation.Logger`. O pacote copia um `nlog.config` inicial sem substituir um arquivo já customizado. No `Program.cs`:

```csharp
LoggerManager.LoadConfigurationFromFile(
    Path.Combine(builder.Environment.ContentRootPath, "nlog.config"));
builder.Services.ConfigureLoggerService();
```

Injete `ILoggerManager` nos serviços. Prefira mensagens com placeholders nomeados e propriedades estruturadas, sem registrar senhas, tokens ou dados pessoais desnecessários.

> **Exemplo no Vault — onde o log vive (e onde não vive).** Pelo ADR 0024, a **Application não loga nem relança exceções**: não há `try/catch` só para registrar e propagar. O registro de falhas acontece nos *exception handlers* da Api, que são o ponto único de log de erros. Isso evita a mesma falha aparecer duplicada em cada camada.
>
> Configuração NLog usada pelo Vault (resumida): grava JSON estruturado, com rotação por tamanho (10 MB) e 30 arquivos de histórico:
>
> ```xml
> <target xsi:type="File" name="jsonFile" fileName="${basedir}/logs/current.json"
>         archiveFileName="${basedir}/logs/archive/json/${shortdate}_logfile.{#}.json"
>         archiveAboveSize="10485760" maxArchiveFiles="30" archiveNumbering="Rolling" createDirs="true">
>   <layout xsi:type="JsonLayout" includeEventProperties="true">
>     <attribute name="time" layout="${longdate}" />
>     <attribute name="level" layout="${level}" />
>     <attribute name="message" layout="${message}" />
>     <attribute name="exception" layout="${exception:format=tostring}" />
>   </layout>
> </target>
> <rules><logger name="*" minlevel="Debug" writeTo="jsonFile" /></rules>
> ```
>
> **Pontos de atenção** observados nesse arquivo: (a) o alvo `logfile` (texto) está declarado, mas nenhuma regra grava nele; (b) combinar `includeEventProperties="true"` com o atributo `properties` (`${all-event-properties}`) tende a duplicar as propriedades; (c) `minlevel="Debug"` é adequado em desenvolvimento, mas em produção costuma ser `Information`; (d) o `nlog.config` está na pasta `Application/`, camada que não deveria conhecer logger. Provavelmente ficou ali por ter sido copiado quando o pacote foi instalado; ele deve morar no projeto host (Api/Service). Vale confirmar.

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

> **Exemplo no Vault — registro por convenção.** No Vault, o *Service* chama `AddServicesFromAssembly` sobre o assembly da Application. A convenção é `I<Classe>` → `<Classe>`: criar `IGetObjetivoQuery` + `GetObjetivoQuery` na pasta certa já basta; **não é preciso registrar nada à mão**. Isso funciona porque interface e implementação estão no mesmo assembly (diferente dos repositórios, ver seção 2).

> **Exemplo no Vault — quem traduz erro de domínio em HTTP.** Os casos de uso lançam exceções do `Core`; a borda converte. O mapa mental, coerente com o código dos comandos:
>
> | Exceção de domínio | Situação típica no Vault | Resposta esperada |
> | --- | --- | --- |
> | `CommandValidationException` / falha em `CommandValidator` | Data futura em cotação; `Quantidade` ausente | 422 |
> | `EntityNotFoundException` | Movimento inexistente **ou de outro usuário** | 404 |
> | `EntityInUseException` | Excluir Corretora com Movimentos | 409 |
>
> Confirme os códigos exatos no README do `AspNetCore.Api`: o guia não os define e este quadro descreve a intenção observada no código da Application.
>
> **Armadilha de consistência:** mensagens de exceção misturam português (`"Movimento não encontrado"`) e inglês (`"Produto not found"`, `"Emissor not found"` nos comandos de atualização). Como as mensagens chegam ao cliente, padronize um idioma por projeto.

> **Exemplo no Vault — regras fixas para todo caso de uso** (README da Application):
>
> - Receber `ownerId` e repassá-lo ao repositório.
> - `CommandValidator.Validate(...)` antes de qualquer acesso a dados.
> - `CancellationToken` obrigatório, último parâmetro, repassado até o `SaveAsync` (ADR 0017).
> - Sem `try/catch` para logar e relançar (ADR 0024); capturar exceção só para tratá-la de fato.

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

> **Exemplo no Vault — identidade isolada do domínio.** O Identity roda em um contexto próprio (`VaultIdentityDbContext`, tabelas `AspNetUsers`, `AspNetRoles` etc.) com migrations próprias. O domínio **não tem tabela de usuário**: ele só guarda o `OwnerId` (um `Guid`) em cada registro. Houve uma fase inicial em que `VaultDbContext` tinha uma entidade `User` com chaves estrangeiras de cascata; a migration `RemocaoDeUser` a eliminou. Vantagens da separação: o domínio não depende de Identity, e é possível trocar o provedor de autenticação sem tocar nas tabelas de negócio.
>
> Contrapartida a documentar: sem chave estrangeira entre `OwnerId` e `AspNetUsers`, a integridade (e a remoção de dados de um usuário excluído) passa a ser responsabilidade da aplicação.
>
> Na configuração de conexão, o `VaultIdentityDbContext` também é exposto como `IdentityContext`:
>
> ```csharp
> services.ConfigureMariaDbContext<VaultIdentityDbContext>(configuration);
> services.AddScoped<IdentityContext>(provider => provider.GetRequiredService<VaultIdentityDbContext>());
> ```

> **Armadilha — valores padrão de credenciais no código.** `ServiceExtensions.BuildDbConnectionHelper` usa *fallbacks* como `"example-user"` e `"my_cool_secret"` quando a variável de ambiente não existe. São valores de desenvolvimento (porta `3307`, banco `my_database`), mas ficam no código-fonte e, se um ambiente real subir sem configurar `VAULT_DB_PASS`, a aplicação tentará conectar com a senha de exemplo em vez de falhar. Alternativas: (1) remover o *fallback* de usuário/senha e falhar rápido fora de `Development`; (2) manter os padrões apenas em `appsettings.Development.json`/`docker compose`. Isso também atende à restrição do projeto de nunca depender de segredos em texto puro.

### 6. Construa consumidores de API

Em um projeto que consome a API, registre `AddLumiaApiClient` e forneça uma implementação de `ITokenStore` adequada ao tipo de host. `InMemoryTokenStore` é apropriado para testes, scripts e processos únicos; aplicações multiusuário precisam de armazenamento associado ao usuário.

```csharp
builder.Services.AddLumiaApiClient("https://api.example.com");
builder.Services.AddApiResourceClient<IOrderApi, OrderApi>();
```

Use `IApiConnection` nos clientes de recurso. Respostas HTTP malsucedidas geram `ApiException`; trate o `StatusCode` sem duplicar a lógica de serialização de erros.

> **Exemplo no Vault.** O `Vault.Web` (Razor Pages) é o consumidor, e a regra de ouro é que ele **nunca** acessa o banco: só chama a Api. A integração com `Http.Client` e `AspNetCore.Web` ainda não foi implementada, então esta seção do guia será completada com exemplos reais quando o front-end existir (cliente de recurso para Movimentos/Objetivos, `ITokenStore` por usuário e a paginação por tag helper sobre `PagedResponse<T>`).

## Convenções de uso

- Propague `CancellationToken` em consultas, chamadas HTTP e `SaveAsync`.
- Não exponha diretamente entidades de persistência: use DTOs na API.
- Padronize erros com `ErrorDetails` e mantenha mensagens apropriadas para o cliente.
- Use health checks de liveness e readiness; marque a conexão do banco com a tag usada pelo readiness.
- Escreva testes unitários para domínio e serviços e testes de integração para persistência, pipeline HTTP e autenticação.
- Consulte os READMEs dos pacotes para exemplos completos: `LumiaFoundation.Core/docs/readme.md`, `LumiaFoundation.EFRepository/docs/readme.md` e os demais diretórios `docs`.

### Convenções adicionais observadas no Vault

- **Multi-tenant por construção:** todo caso de uso e todo método de repositório recebem `ownerId`; índices compostos começam por `OwnerId`.
- **Um `ModelForCreation`/`ModelForUpdate`/`Model` por agregado**, com `ToDomain`/`UpdateDomain`/`FromDomain`.
- **Nomes de tipos de caso de uso:** `<Verbo><Agregado>Command` e `Get<Agregado>[List]Query`, em pastas `Commands/` e `Queries/` por agregado.
- **Comentários e ADRs explicam o "porquê":** consultas sensíveis a desempenho apontam o ADR que justifica ordenação e índice (por exemplo, ADR 0009, 0010, 0015, 0019).
- **Documentação viva:** cada projeto tem README com dependências permitidas, estrutura, regras e "como adicionar um caso de uso/repositório".
- **Testes:** `Application.Test` com `Moq` ou um `FakeRepositoryManager` (caminho feliz, validação, propagação de exceção e cancelamento); `Persistence.Test` com `Microsoft.EntityFrameworkCore.InMemory` e um teste de que dados de outro `OwnerId` não aparecem; teste de arquitetura para as dependências entre camadas.
  - *Ressalva:* o provedor InMemory não aplica restrições `CHECK`, chaves estrangeiras nem o comportamento de índices únicos. Regras que dependem disso (como `CK_Movimentos_Encerramento` e o índice único de `Cotacoes`) merecem um teste de integração contra um MariaDB real (por exemplo, em contêiner).

### Mais dois padrões reaproveitáveis

> **Exemplo no Vault — defesa em profundidade para unicidade.** Para impedir duas cotações do mesmo papel na mesma data, há (1) um índice único `IX_Cotacoes_OwnerId_Papel_Data` e (2) uma checagem prévia no caso de uso com mensagem amigável:
>
> ```csharp
> // Application/Cotacoes/Commands/CreateCotacao/CreateCotacaoCommand.cs (trecho)
> if (await r.Cotacao.GetPorPapelEDataAsync(ownerId, c.Papel, d, false, ct) != null)
>     throw new CommandValidationException("Já existe cotação para este papel nesta data. Edite o registro existente.");
> ```
>
> A checagem dá a boa mensagem; o índice garante a integridade sob concorrência. Ponto em aberto: se duas requisições passarem pela checagem ao mesmo tempo, a segunda falhará no banco com `DbUpdateException`. Convém traduzir essa exceção para a mesma resposta 422/409.

> **Exemplo no Vault — upsert que dispara uma regra em lote.** `AtualizarPorCotacaoCommand` cria ou atualiza a cotação do dia e, só se ela for a mais recente do papel, recalcula todos os movimentos ativos ligados a esse papel, tudo em um único `SaveAsync`. Mostra bem o valor de uma única unidade de trabalho: a cotação e os movimentos recalculados são confirmados juntos ou nenhum é.

## Checklist de um primeiro incremento

- [ ] Projeto criado e target .NET confirmado.
- [ ] Pacotes mínimos instalados (por camada, sem referências para fora).
- [ ] Entidade e regra de domínio implementadas (regras na entidade, não no controller).
- [ ] Contexto, configurações EF e migration criados.
  - [ ] Todo `decimal` com `HasPrecision`; todo `string` com `HasMaxLength`.
  - [ ] Seeds com chaves determinísticas.
  - [ ] Índices compostos começando pelo identificador do dono, quando houver multi-tenant.
- [ ] Repositório e caso de uso testados (incluindo teste de isolamento entre dois `OwnerId`).
- [ ] Logger e tratamento global de exceções configurados (um único ponto de log de erros).
- [ ] Endpoint com DTO, validação e paginação, se aplicável (dono vindo do token, nunca do corpo).
- [ ] Health checks, OpenAPI e autenticação configurados conforme necessidade.
- [ ] Segredos fora do repositório e sem valores padrão sensíveis no código.
- [ ] Testes executados com `dotnet test LumiaFoundation.slnx` (na biblioteca) ou `dotnet test <Projeto>.Test` (na aplicação).
- [ ] README do projeto e ADRs das decisões relevantes atualizados.

## Pontos a validar nos pacotes (candidatos a melhoria no próprio Lumia.Foundation)

Observações feitas ao comparar este guia com o uso real no Vault, que podem virar melhorias da biblioteca ou da documentação:

1. **Provedor MariaDB:** o Vault usa `ConfigureMariaDbDatabase<T>`, mas o guia cita só `ConfigureMySqlDbDatabase<T>` e `ConfigurePostgreSqlDatabase<T>`. Documentar o método do MariaDB (e quando escolhê-lo).
2. **Precisão padrão de `decimal`:** `RepositoryContext` poderia aplicar uma convenção (por exemplo, `decimal(18,2)`) para evitar o `decimal(65,30)` que gerou duas migrations corretivas no Vault.
3. **Validação de `Guid` vazio:** destacar `[NotEmptyGuid]` no guia e alertar que `[Required]` não protege `Guid`.
4. **Mapeamento exceção → HTTP:** incluir no guia uma tabela oficial (exceção de domínio → código HTTP) para os projetos não precisarem deduzir.
5. **Seed de roles:** oferecer em `Lumia.Foundation.Auth` uma `RoleConfiguration` pronta, com `Id`/`ConcurrencyStamp` determinísticos, para evitar a armadilha do `HasData` aleatório.
6. **Cenário de dois contextos** (domínio + Identity): documentar a estrutura de pastas de migrations, as duas `IDesignTimeDbContextFactory` e o registro `IdentityContext` → `VaultIdentityDbContext`.
7. **Teste de arquitetura:** publicar um modelo reutilizável do `DependenciasEntreCamadasTests`.
8. **Tradução de `DbUpdateException` por violação de unicidade** em exceção de domínio, para o cenário de concorrência descrito acima.

## Evolução deste guia

Este documento será ampliado iterativamente a partir da experiência dos projetos que usam o Lumia.Foundation. Ao descobrir uma convenção, corrigir uma sequência ou adicionar um cenário recorrente, atualize o conteúdo e registre a mudança abaixo. Alterações incompatíveis ou mudanças de orientação devem receber uma nova versão claramente descrita.

## Histórico de versões

### 0.2.0 — 13/11/2026

- Adicionados exemplos reais do projeto Vault em cada etapa da sequência de construção (domínio, persistência, logging, API, autenticação).
- Documentada a organização por camadas do Vault, a inversão de dependência entre Application e Persistence e o teste de arquitetura.
- Adicionadas seções de **Armadilhas** (decimais sem precisão, seed aleatório, renomeação de colunas, dono vindo do corpo da requisição, credenciais padrão no código, mensagens em dois idiomas).
- Incluídos padrões reaproveitáveis (invariante também no banco, defesa em profundidade para unicidade, migration com migração de dados).
- Ampliado o checklist e criada a lista de pontos a validar nos pacotes.
- Marcada como pendente a seção de consumidores de API, a completar quando o `Vault.Web` for implementado.

### 0.1.0 — 13/11/2026

- Criado o guia inicial para desenvolvedores iniciantes.
- Documentados os pacotes, suas responsabilidades e a combinação recomendada.
- Adicionada uma sequência de construção de domínio, persistência, infraestrutura, API, autenticação e clientes.
- Adicionados exemplos mínimos de instalação, configuração e checklist.
- Registrada a expectativa de evolução do documento conforme novos projetos forem desenvolvidos.
