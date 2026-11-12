# Inventário: divisão de `Lumia.Foundation.AspNetCore` em `.Api` e `.Web`

Base: LumiaFoundation `main`, `LumiaFoundation.AspNetCore` 0.31.0. Os namespaces `LumiaFoundation.AspNetCore.*` **não mudam**: no Vault só mudam as `PackageReference`.

## 1. Achado que ajusta a proposta

A "base compartilhada" (opção C) ficaria com **uma única coisa**: `AddServicesFromAssembly`. O Presentation não a usa (só `AddApiSignInService` e `AddApiRegistrationService`); só o Service a usa. Um pacote para um método não se justifica. Proposta ajustada: **dois pacotes, sem base**.

- `AddServicesFromAssembly` vai para o `.Api`.
- `Lumia.Foundation.AspNetCore` é descontinuado (a última versão, 0.31.x, continua disponível no NuGet, marcada como obsoleta, com aviso apontando para os dois novos).
- Se um Web futuro precisar da convenção por assembly, extrai-se uma base nesse momento, com a necessidade real.

## 2. Arquivos para `Lumia.Foundation.AspNetCore.Api`

| Arquivo (origem) | Observação |
|---|---|
| `ActionFilters/DtoNotEmptyValidationAttribute.cs` | |
| `ActionFilters/RequestValidationAttribute.cs` | usa `HttpBaseException` |
| `Commons/BaseControllers/BaseApiController.cs` | |
| `Commons/Exceptions/HttpBaseException.cs` | decisão 3: o Web usa `Http.Client.Exceptions` |
| `Commons/Extensions/HealthCheckEndpointExtensions.cs` | |
| `Commons/Extensions/OpenApiEndpointRouteBuilderExtensions.cs` | Scalar |
| `Commons/Extensions/OpenApiServiceExtensions.cs` | inclui o `internal` `ApiInfoDocumentTransformer` |
| `Commons/Extensions/ServiceCollectionExtensions.cs` | **dividir** (item 4): ficam `AddServicesFromAssembly`, `AddValidationFilters`, `AddExceptionHandlers` |
| `ExceptionHandlers/DomainExceptionHandler.cs` | |
| `ExceptionHandlers/UnhandledExceptionHandler.cs` | |
| `HealthChecks/LumiaHealthCheckPaths.cs` | |
| `HealthChecks/LumiaHealthCheckTags.cs` | |
| `Pagination/PagedListExtensions.cs` | converte `PagedList` em metadados da resposta; o Service a usa |

## 3. Arquivos para `Lumia.Foundation.AspNetCore.Web`

| Arquivo (origem) | Observação |
|---|---|
| `ClientAuthentication/ApiRegistrationService.cs` | |
| `ClientAuthentication/ApiSessionCookieEvents.cs` | |
| `ClientAuthentication/ApiSignInService.cs` | |
| `ClientAuthentication/ApiUnauthorizedExceptionHandler.cs` | |
| `ClientAuthentication/ApiUnauthorizedExceptionHandlerOptions.cs` | |
| `ClientAuthentication/IApiRegistrationService.cs` | |
| `ClientAuthentication/IApiSignInService.cs` | |
| `ClientAuthentication/RegistrationOutcome.cs` | |
| `ClientAuthentication/RegistrationResult.cs` | |
| `ClientAuthentication/SessionTokenStore.cs` | |
| `ClientAuthentication/SessionTokenStoreOptions.cs` | |
| `ClientAuthentication/SignInOutcome.cs` | |
| `Pagination/PaginationOptions.cs` | usada só pelo tag helper |
| `Pagination/PaginationRenderer.cs` | `internal` |
| `Pagination/PaginationTagHelper.cs` | |
| `Pagination/PaginationWindow.cs` | `internal` |
| `InternalsVisibleTo.cs` | apontar para `LumiaFoundation.AspNetCore.Web.Test` (por causa de `PaginationRenderer` e `PaginationWindow`) |

Total: 13 arquivos no `.Api` e 17 no `.Web`, somando 30 arquivos, o que cobre todos os `.cs` do projeto atual.

## 4. Arquivo a dividir: `ServiceCollectionExtensions`

| Método | Destino |
|---|---|
| `AddServicesFromAssembly` | Api |
| `AddValidationFilters` | Api |
| `AddExceptionHandlers` | Api |
| `AddApiSignInService` | Web |
| `AddApiRegistrationService` | Web |

Os dois métodos do Web ganham uma classe própria, por exemplo `ClientAuthenticationServiceExtensions`, **no mesmo namespace** `LumiaFoundation.AspNetCore.Commons.Extensions`, para o `using` do `Presentation/Program.cs` continuar valendo. Duas classes de extensão em assemblies diferentes, no mesmo namespace, é válido e não causa ambiguidade porque os nomes de método são distintos.

## 5. Dependências de cada pacote

| | `.Api` | `.Web` |
|---|---|---|
| `FrameworkReference Microsoft.AspNetCore.App` | sim | sim |
| `Lumia.Foundation.Core` | sim (`DomainExceptionHandler`, `PagedListExtensions`) | não |
| `Lumia.Foundation.Logger` | sim | sim |
| `Lumia.Foundation.Abstractions` | sim (`ErrorDetails`, paginação) | não |
| `Lumia.Foundation.Http.Client` | **não** | sim |
| `Microsoft.AspNetCore.OpenApi` | sim | **não** |
| `Scalar.AspNetCore` | sim | **não** |

O Api deixa de carregar `Http.Client`; o Web deixa de carregar `Scalar` e `OpenApi`. Confirmar no build se o `.Web` precisa de `Core` por algum tipo não listado.

## 6. Projetos de teste

Dividir `LumiaFoundation.AspNetCore.Test` em `.Api.Test` e `.Web.Test`.

| Arquivo (origem em `AspNetCore.Test`) | Destino |
|---|---|
| `ActionFilters/*` (2) | Api.Test |
| `Commons/Exceptions/AddServicesFromAssemblyTests.cs` | Api.Test |
| `Commons/Exceptions/HttpBaseExceptionTests.cs` | Api.Test |
| `Commons/Extensions/HealthCheckEndpointExtensionsTests.cs` | Api.Test |
| `ExceptionHandlers/DomainExceptionHandlerTests.cs`, `UnhandledExceptionHandlerTests.cs` | Api.Test |
| `ExceptionHandlers/ErrorModel/ErrorDetailsTests.cs` | Api.Test (testa tipo da `Abstractions`; candidato a sair daqui depois) |
| `Pagination/PagedListExtensionsTests.cs` | Api.Test |
| `ClientAuthentication/*` (6, incluindo `FakeSession.cs`) | Web.Test |
| `Pagination/PaginationRendererTests.cs`, `PaginationTagHelperTests.cs`, `PaginationWindowTests.cs` | Web.Test |
| `TestDoubles/FakeLoggerManager.cs` | **os dois** (cópia em cada projeto) |

O `Microsoft.AspNetCore.TestHost` hoje está no projeto de testes único. Cada novo projeto leva só se algum teste o usar (verificar no build). `.Web.Test` referencia `Http.Client`; `.Api.Test` não.

## 7. Outros arquivos do repositório

- `LumiaFoundation.slnx`: trocar as duas entradas atuais por quatro (`.Api`, `.Web` e seus testes).
- `.github/workflows/publish-nuget.yml`: detecta pacotes pela diferença de `PackageVersion` desde a tag anterior. Os dois pacotes novos não têm versão na tag base; **confirmar** que o script trata projeto sem versão anterior como "novo" e o publica.
- `LumiaFoundation.AspNetCore/docs/readme.md` (464 linhas): dividir por seção. Api: registro automático, tratamento de exceções, controller base, health checks, filtros de ação e OpenAPI/Scalar. Web: consumo da API protegida (`Http.Client`), registro típico e paginação (TagHelper). O "Resposta paginada da Api" vai para o Api. O histórico de versões fica no pacote antigo, com uma nota de migração.
- `readme.md` da raiz: tabela de pacotes, `dotnet add package` e o grafo (`AspNetCore --> Core/Logger/Abstractions`), que passa a ter dois nós.
- `LumiaFoundation.AspNetCore/nlog.config`: está no projeto mas parece resquício (o Logger tem a configuração); decidir se vai para algum dos novos ou é descartado.
- Versão inicial dos pacotes novos: sugestão `0.32.0` nos dois, para a sequência do pacote original ficar legível. Decisão sua.

## 8. Impacto no Vault

| Projeto | Mudança |
|---|---|
| `Service.csproj` | `Lumia.Foundation.AspNetCore` → `Lumia.Foundation.AspNetCore.Api` (mantém `Logger`) |
| `Presentation.csproj` | `Lumia.Foundation.AspNetCore` → `Lumia.Foundation.AspNetCore.Web` (mantém `Logger` e `Http.Client`) |
| Código C# e Razor | nenhuma mudança, por causa dos namespaces preservados |
| `Presentation/Pages/_ViewImports.cshtml` (linha 3) | `@addTagHelper *, LumiaFoundation.AspNetCore` cita o **assembly** e precisa virar `@addTagHelper *, LumiaFoundation.AspNetCore.Web` (nome do assembly = nome do projeto, salvo `AssemblyName` explícito) |
| `DependenciasEntreCamadasTests` | acrescentar regras: Service proíbe `Lumia.Foundation.AspNetCore.Web` e Presentation proíbe `Lumia.Foundation.AspNetCore.Api` |

O `@addTagHelper` é o único ponto que não é só troca de pacote. Sem ele o `<pagination>` deixa de ser reconhecido e vira tag HTML desconhecida, sem erro de compilação: validar abrindo a listagem de Movimentos.

## 9. Ordem sugerida

1. Criar os dois projetos e mover os arquivos (você já criou os projetos). Build de cada um.
2. Dividir `ServiceCollectionExtensions` e os projetos de teste. Testes verdes nos dois.
3. Dividir os readmes e atualizar `.slnx` e a raiz.
4. Publicar `.Api` e `.Web`; marcar o `AspNetCore` como obsoleto.
5. No Vault: trocar as referências, ajustar o `@addTagHelper`, estender o teste de arquitetura, rodar build, testes e `docker compose up --build`.
