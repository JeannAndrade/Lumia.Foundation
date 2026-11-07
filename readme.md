# Lumia.Foundation

Conjunto de bibliotecas .NET reutilizáveis para acelerar o desenvolvimento de APIs ASP.NET Core: abstrações de domínio, repositório genérico sobre EF Core, autenticação JWT/Identity, cliente HTTP tipado, logging estruturado e tratamento padronizado de erros.

Cada pacote é publicado individualmente no NuGet e pode ser usado isoladamente ou em conjunto.

## Pacotes

| Pacote | Descrição | Versão | Docs |
| :--- | :--- | :--- | :--- |
| `Lumia.Foundation.Core` | Entidades e exceções de domínio, validação com DataAnnotations, provedores de data/hora e paginação com `PagedList<T>`. Base para os demais pacotes. | 0.13.0 | [readme](LumiaFoundation.Core/docs/readme.md) |
| `Lumia.Foundation.Abstractions` | Contratos compartilhados `ErrorDetails` e `PagedResponse<T>` para respostas padronizadas de erro e paginação. | 0.2.0 | [readme](LumiaFoundation.Abstractions/docs/readme.md) |
| `Lumia.Foundation.Logger` | Interface de logging (`ILoggerManager`) baseada em NLog, registro via injeção de dependência e configuração inicial copiada sem sobrescrever arquivos existentes. | 0.5.0 | [readme](LumiaFoundation.Logger/docs/readme.md) |
| `Lumia.Foundation.EFRepository` | Contexto e repositórios genéricos, configuração para MySQL/MariaDB/PostgreSQL, health checks do banco e persistência com `CancellationToken`. | 0.14.0 | [readme](LumiaFoundation.EFRepository/docs/readme.md) |
| `Lumia.Foundation.AspNetCore` | Handlers globais de exceção, filtros de validação, `BaseApiController`, registro automático de serviços, health checks de liveness/readiness e OpenAPI/Scalar. | 0.30.1 | [readme](LumiaFoundation.AspNetCore/docs/readme.md) |
| `Lumia.Foundation.Auth` | ASP.NET Core Identity, geração e validação de JWT, autenticação e renovação de tokens com refresh token. | 0.3.0 | [readme](LumiaFoundation.Auth/docs/readme.md) |
| `Lumia.Foundation.Http.Client` | Cliente HTTP tipado com `HttpClientFactory`, desserialização JSON, erros como `ApiException` e refresh automático de JWT após HTTP 401. | 0.5.1 | [readme](LumiaFoundation.Http.Client/docs/readme.md) |

## Requisitos

- **.NET 10.0**
- MySQL, MariaDB ou PostgreSQL, caso use `Lumia.Foundation.EFRepository`/`Lumia.Foundation.Auth`

## Instalação

Instale apenas os pacotes que o projeto precisa:

```bash
dotnet add package Lumia.Foundation.Core
dotnet add package Lumia.Foundation.Abstractions
dotnet add package Lumia.Foundation.Logger
dotnet add package Lumia.Foundation.EFRepository
dotnet add package Lumia.Foundation.AspNetCore
dotnet add package Lumia.Foundation.Auth
dotnet add package Lumia.Foundation.Http.Client
```

Cada pacote tem seu próprio guia de configuração linkado na tabela acima.

## Como os pacotes se relacionam

```mermaid
graph TD
    Core[Lumia.Foundation.Core]
    Abstractions[Lumia.Foundation.Abstractions]
    Logger[Lumia.Foundation.Logger]
    EFRepository[Lumia.Foundation.EFRepository]
    AspNetCore[Lumia.Foundation.AspNetCore]
    Auth[Lumia.Foundation.Auth]
    HttpClient[Lumia.Foundation.Http.Client]

    EFRepository --> Core
    AspNetCore --> Core
    AspNetCore --> Logger
    AspNetCore --> Abstractions
    Auth --> Logger
    HttpClient --> Abstractions
```

`Core`, `Abstractions` e `Logger` não têm dependências entre si e servem de base para os demais pacotes.

## Estrutura do repositório

```
LumiaFoundation.Core/           Abstrações de domínio e utilitários
LumiaFoundation.Abstractions/   Modelo de erro compartilhado
LumiaFoundation.Logger/         Logging estruturado (NLog)
LumiaFoundation.EFRepository/   Repositório genérico sobre EF Core
LumiaFoundation.AspNetCore/     Utilitários para APIs ASP.NET Core
LumiaFoundation.Auth/           Identity + JWT
LumiaFoundation.Http.Client/    Cliente HTTP tipado
*.Test/                         Projetos de teste (xUnit) de cada pacote
LumiaFoundation.slnx            Solution com todos os projetos
```

## Testes

```bash
dotnet test LumiaFoundation.slnx
```

## Licença

Distribuído sob a licença definida em [`LICENSE`](LICENSE).

> **Nota:** os `.csproj` dos pacotes atualmente declaram `PackageLicenseExpression = GPL-3.0-or-later` no metadata do NuGet, enquanto o arquivo `LICENSE` da raiz é MIT. Vale alinhar os dois antes da próxima publicação, para não gerar inconsistência entre o que aparece no NuGet.org e o que está no repositório.

## Autor

**Jeann Andrade** — Lumia Tech
