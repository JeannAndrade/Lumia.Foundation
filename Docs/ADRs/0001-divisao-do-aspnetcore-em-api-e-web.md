# ADR 0001: Divisão de `Lumia.Foundation.AspNetCore` em `.Api` e `.Web`

- Status: Aceita (2026-11-12). Versão inicial implementada em `0.32.0`; o pacote legado permanece disponível como obsoleto.

## Contexto

`Lumia.Foundation.AspNetCore` (0.31.0) atende dois tipos de aplicação com necessidades distintas, e cada uma carrega o que é da outra:

- **Api** (no Vault, o Service): `BaseApiController`, filtros de ação, `HttpBaseException`, handlers de exceção, health checks, OpenAPI/Scalar, `PagedListExtensions` e `AddServicesFromAssembly`.
- **Web** (no Vault, o Presentation): autenticação de cliente por sessão e cookie (`ClientAuthentication`, 12 arquivos) e o tag helper de paginação.

Consequências do pacote único:

- O Web recebe `Scalar.AspNetCore`, `Microsoft.AspNetCore.OpenApi`, controllers base e handlers de domínio que não usa.
- A Api recebe `Lumia.Foundation.Http.Client` e toda a autenticação de cliente que não usa.
- Todo projeto novo (o próximo é o Flashcard) herda o pacote inteiro, e o teste de arquitetura do consumidor não consegue separar "Api" de "Web" por pacote.

Não é um problema de isolamento de framework: as duas aplicações são ASP.NET Core e usam `Microsoft.AspNetCore.App`. O ganho é coesão e dependências mínimas.

Verificado no código: o único trecho realmente compartilhado seria `AddServicesFromAssembly`, e o Web (Presentation) não o usa. Inventário arquivo por arquivo: `lumia-aspnetcore-api-web-inventario.md`.

## Decisão

1. Criar dois pacotes: **`Lumia.Foundation.AspNetCore.Api`** e **`Lumia.Foundation.AspNetCore.Web`**, cada um com seu projeto de testes (`.Api.Test` e `.Web.Test`).
2. **Sem pacote base.** `AddServicesFromAssembly` vai para o `.Api`. Se um Web precisar da convenção por assembly, extrai-se uma base nesse momento.
3. **Namespaces preservados** (`LumiaFoundation.AspNetCore.*`): no consumidor muda a `PackageReference`, não o código C#.
4. Divisão do conteúdo:
   - `.Api`: `ActionFilters`, `BaseApiController`, `HttpBaseException`, `ExceptionHandlers`, `HealthChecks` e suas extensões, extensões de OpenAPI/Scalar, `PagedListExtensions`, e em `ServiceCollectionExtensions` os métodos `AddServicesFromAssembly`, `AddValidationFilters` e `AddExceptionHandlers`.
   - `.Web`: `ClientAuthentication/*`, `PaginationTagHelper` com `PaginationOptions`, `PaginationRenderer` e `PaginationWindow`, e os métodos `AddApiSignInService` e `AddApiRegistrationService`, movidos para uma classe própria no mesmo namespace.
5. Dependências: `.Api` usa Core, Logger, Abstractions, OpenApi e Scalar (sem `Http.Client`); `.Web` usa Logger e `Http.Client` (sem Scalar nem OpenApi).
6. **`Lumia.Foundation.AspNetCore` é descontinuado:** a última versão (0.31.x) permanece no NuGet, marcada como obsoleta com aviso apontando para os dois novos.
7. Versão inicial dos novos pacotes: `0.32.0`, para a sequência do pacote original ficar legível.

## Alternativas consideradas

- **Manter um pacote:** custo zero, mas mantém o excesso de dependências e a falta de coesão, e o Flashcard herdaria o pacote inteiro.
- **Três pacotes (base + Api + Web):** a base ficaria com um único método, sem justificar um pacote a mais para versionar e publicar.
- **Dois pacotes independentes com o compartilhado duplicado:** evita a base, mas duplicaria código; descartado porque o compartilhado se reduziu a um método de um só lado.
- **Mover `AddServicesFromAssembly` para o `Core`:** o Core é referenciado pelo Domain e ganharia dependência de DI sem necessidade.

## Consequências

- **Mudança de contrato público**, com impacto em Web e Api: `Service.csproj` troca para `.Api`; `Presentation.csproj` troca para `.Web`. O código C# não muda.
- **Ponto que não é só troca de pacote:** `@addTagHelper *, LumiaFoundation.AspNetCore` (em `_ViewImports.cshtml`) cita o assembly e precisa virar `*, LumiaFoundation.AspNetCore.Web`. Sem isso, o `<pagination>` vira tag HTML desconhecida, sem erro de compilação.
- Os dois métodos do Web ficam em classe própria, no namespace `LumiaFoundation.AspNetCore.Commons.Extensions`, também usado pelo `.Api`. Dois assemblies com o mesmo namespace são válidos, e não há ambiguidade porque os nomes dos métodos são distintos.
- `FakeLoggerManager` é copiado nos dois projetos de teste. `InternalsVisibleTo` vai para o `.Web` (`PaginationRenderer` e `PaginationWindow` são `internal`).
- O workflow `publish-nuget.yml` detecta pacotes pela diferença de `PackageVersion` desde a tag anterior; é preciso confirmar que trata projeto sem versão anterior como novo.
- O consumidor ganha regras simples de arquitetura: a Api proíbe `.Web` e o Web proíbe `.Api` (no Vault, em `DependenciasEntreCamadasTests`).
- `readme.md` da raiz (tabela de pacotes e grafo) e `docs/readme.md` do pacote são divididos; o histórico de versões fica no pacote antigo, com nota de migração.

## Como validar

1. `dotnet build` e `dotnet test` na solução da LumiaFoundation, verdes, com os quatro projetos novos.
2. `dotnet list <projeto> package --include-transitive`: `.Api` sem `Http.Client`; `.Web` sem Scalar nem OpenApi.
3. No Vault, após trocar as referências e o `@addTagHelper`: `dotnet build`, `dotnet test` (incluindo `DependenciasEntreCamadasTests` com as novas regras) e `docker compose up --build`.
4. Abrir a listagem de Movimentos no Web e conferir que a paginação é renderizada.

## Ordem de execução

1. Mover os arquivos e compilar cada projeto novo.
2. Dividir `ServiceCollectionExtensions` e os projetos de teste.
3. Dividir os readmes e atualizar a `.slnx` e o `readme.md` da raiz.
4. Publicar `.Api` e `.Web`; marcar o `AspNetCore` como obsoleto.
5. Atualizar o Vault.
