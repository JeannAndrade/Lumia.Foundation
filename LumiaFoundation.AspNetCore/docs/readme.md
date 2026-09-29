# Lumia.Foundation.AspNetCore

Biblioteca com classes utilitárias para aplicações ASP.NET Core, incluindo tratamento global de exceções, filtros de ação, registro automático de serviços e documentação com OpenAPI e Scalar.

## Instalação

Instale o pacote no projeto ASP.NET Core:

```bash
dotnet add package Lumia.Foundation.AspNetCore
```

O pacote depende de `Lumia.Foundation.Core` e `Lumia.Foundation.Logger`, e utiliza ASP.NET Core 10 com Microsoft.AspNetCore.OpenApi e Scalar.AspNetCore.

## Registro automático de serviços

O método `AddServicesFromAssembly` registra classes concretas que implementam uma interface com o padrão `I{NomeDaClasse}`. O tempo de vida padrão é `Scoped`:

```csharp
using LumiaFoundation.AspNetCore.Commons.Extensions;

builder.Services.AddServicesFromAssembly(typeof(OrderService).Assembly);
```

Para usar outro `ServiceLifetime`, informe-o explicitamente:

```csharp
builder.Services.AddServicesFromAssembly(
 typeof(OrderService).Assembly,
 ServiceLifetime.Transient);
```

Neste exemplo, `OrderService` será registrado quando implementar `IOrderService`:

```csharp
public interface IOrderService
{
}

public class OrderService : IOrderService
{
}
```

## Tratamento de exceções

### Handler recomendado

Registre os dois handlers no pipeline de exceções. O `DomainExceptionHandler` trata exceções de domínio e o `UnhandledExceptionHandler` trata as demais, retornando HTTP 500:

```csharp
using LumiaFoundation.AspNetCore.ExceptionHandlers;

builder.Services.AddExceptionHandler<DomainExceptionHandler>();
builder.Services.AddExceptionHandler<UnhandledExceptionHandler>();

var app = builder.Build();
app.UseExceptionHandler();
```

Os handlers retornam respostas JSON com `StatusCode`, `Message` e `ExceptionType`.

Exceções de domínio devem herdar de `DomainBaseException`, disponibilizada pelo pacote `Lumia.Foundation.Core`. Essa classe não possui dependências de HTTP e pode ser usada em qualquer camada da aplicação.

Quando uma exceção precisar representar uma resposta HTTP, herde de `HttpBaseException`, específica deste pacote, e defina o status HTTP em `StatusCodeValue`:

```csharp
using LumiaFoundation.Core.Domain.Exceptions;
using LumiaFoundation.AspNetCore.Commons.Exceptions;
using Microsoft.AspNetCore.Http;

public sealed class UserNotFoundException : HttpBaseException
{
 public UserNotFoundException()
    : base(StatusCodes.Status404NotFound, "User not found.")
 {
 }
}
```

Para validação de comandos, use `CommandValidator` e `CommandValidationException` do `Lumia.Foundation.Core`:

```csharp
using LumiaFoundation.Core.Domain.Exceptions;
using LumiaFoundation.Core.Validators;

CommandValidator.Validate(command);

public sealed class InvalidUserCommandException : CommandValidationException
{
 public InvalidUserCommandException(string message) : base(message)
 {
 }
}
```

O código HTTP não é passado pelo construtor da classe base. O `DomainExceptionHandler` trata `HttpBaseException`; exceções de domínio sem representação HTTP são encaminhadas para o tratamento genérico. Exceções com status entre 400 e 499 são registradas como aviso; status entre 500 e 599 são registrados como erro.

### Controller base

`BaseApiController` pode ser usado como filtro de ação para converter exceções de domínio do `Lumia.Foundation.Core` em `HttpBaseException`:

```csharp
using LumiaFoundation.AspNetCore.Commons.BaseControllers;

public class OrdersController : BaseApiController
{
}
```

O mapeamento padrão retorna HTTP 404 para `EntityNotFoundException`, HTTP 422 para `CommandValidationException` e HTTP 500 para outras exceções de domínio ou exceções não tratadas. Exceções que já são `HttpBaseException` são preservadas.

## Filtros de ação

### Validação de DTO

`DtoNotEmptyValidationAttribute` verifica se a ação recebeu um DTO e devolve HTTP 400 quando o corpo está vazio ou não contém DTO, e HTTP 422 quando o `ModelState` contém erros:

```csharp
using LumiaFoundation.AspNetCore.ActionFilters;

[ServiceFilter(typeof(DtoNotEmptyValidationAttribute))]
public class OrdersController : ControllerBase
{
}
```

Registre o filtro na injeção de dependência quando usar `ServiceFilter`:

```csharp
builder.Services.AddScoped<DtoNotEmptyValidationAttribute>();
```

## OpenAPI e Scalar

Configure o documento OpenAPI informando o título e a versão da API:

```csharp
using LumiaFoundation.AspNetCore.Commons.Extensions;

builder.Services.ConfigureOpenApi(
 title: "Orders API",
 version: "v1");
```

Depois de criar a aplicação, use `MapOpenApiDevTools` para disponibilizar o documento OpenAPI e a interface Scalar somente em desenvolvimento:

```csharp
using LumiaFoundation.AspNetCore.Commons.Extensions;

var app = builder.Build();
app.MapOpenApiDevTools();
```

Para controlar explicitamente a exposição, informe uma função em `shouldExpose`:

```csharp
app.MapOpenApiDevTools(_ => true);
```

As extensões `MapOpenApiDocuments` e `MapScalarUi` também podem ser usadas separadamente quando a aplicação precisar mapear esses endpoints de forma independente.

## Consumindo uma API protegida por Lumia.Auth (via Http.Client)

Para hosts ASP.NET Core multiusuário (sites, futuros componentes Blazor) que consomem uma API protegida pelo `Lumia.Foundation.Auth` através do `Lumia.Foundation.Http.Client`, o namespace `LumiaFoundation.AspNetCore.ClientAuthentication` oferece:

| Componente | Finalidade |
| :--- | :--- |
| `SessionTokenStore` | Implementação de `ITokenStore` que guarda o par de tokens na sessão (`ISession`) do usuário atual. Registre como `Singleton` — ela não guarda estado próprio. |
| `SessionTokenStoreOptions` | Chave de sessão usada pelo `SessionTokenStore`. Configure uma chave distinta por API quando o host falar com mais de uma. |
| `IApiSignInService` / `ApiSignInService` | Autentica na API, grava o token no `ITokenStore` e emite o sign-in local do host (cookie ou outro esquema configurado). |
| `ApiUnauthorizedExceptionHandler` | `IExceptionHandler` que encerra a sessão do usuário e redireciona ao login quando a API responde 401 e o refresh falha. |
| `ApiSessionCookieEvents` | Evento de cookie (`ValidatePrincipal`) que rejeita o cookie quando o `ITokenStore` não tem mais token — por exemplo, após o host reiniciar. |

### Registro típico

```csharp
builder.Services.AddHttpContextAccessor();
builder.Services.AddDistributedMemoryCache(); // troque por AddStackExchangeRedisCache ao escalar para múltiplos pods
builder.Services.AddSession();

builder.Services.Configure<SessionTokenStoreOptions>(o => o.SessionKey = "MinhaApi.ApiToken");
builder.Services.AddSingleton<ITokenStore, SessionTokenStore>();
builder.Services.AddLumiaApiClient("https://minha-api/");
builder.Services.AddScoped<IApiSignInService, ApiSignInService>();

builder.Services.AddScoped<ApiSessionCookieEvents>();
builder.Services
    .AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Login";
        options.EventsType = typeof(ApiSessionCookieEvents);
    });

builder.Services.AddExceptionHandler<ApiUnauthorizedExceptionHandler>();
```

> A ordem de middlewares importa: `app.UseSession()` precisa vir antes de `app.UseExceptionHandler(...)`, para que o handler ainda consiga limpar a sessão quando tratar uma exceção.

## Histórico de versões

### 0.24.1

- Atualização do package Scalar.AspNetCore 2.17.11

### 0.24.0

- Adicionado IApiRegistrationService/ApiRegistrationService a ClientAuthentication

### 0.23.0

- Adicionada a área `ClientAuthentication`: `SessionTokenStore`, `IApiSignInService`/`ApiSignInService`, `ApiUnauthorizedExceptionHandler` e `ApiSessionCookieEvents`, para hosts ASP.NET Core que consomem uma API protegida pelo `Lumia.Foundation.Auth` via `Lumia.Foundation.Http.Client`.
- Nova dependência: `Lumia.Foundation.Http.Client`.

### 0.22.0 (breaking change)

- Consolidada uma única estratégia de tratamento de exceções: `DomainExceptionHandler` reconhece diretamente `EntityNotFoundException`, `EntityInUseException` e `CommandValidationException`, sem depender de um filtro de MVC pra traduzi-las antes.
- **Removidos** `DomainExceptionMappingFilter`, `AddDomainExceptionMappingFilter` (`Commons/Extensions/DomainExceptionMappingFilterExtensions`) e o middleware legado `ConfigureExceptionHandler` (`Commons/Extensions/ExceptionMiddlewareExtensions`). Migre para `AddExceptionHandler<DomainExceptionHandler>()` + `AddExceptionHandler<UnhandledExceptionHandler>()` + `app.UseExceptionHandler()`.
- `BaseApiController` não aplica mais nenhum filtro de exceção — segue existindo só por `GetCurrentUserId()`.
- Nova propriedade `HttpBaseException.ExceptionType`: expõe o tipo real da exceção (o da `InnerException`, quando existir). `ErrorDetails.ExceptionType` agora mostra `EntityNotFoundException`/`EntityInUseException`/etc. em vez de sempre `"HttpBaseException"`.

### 0.21.0

- `DomainExceptionMappingFilter` passa a mapear `EntityInUseException` (de `Lumia.Foundation.Core`) para HTTP 422 — para regras de domínio que impedem uma ação por existirem entidades dependentes.
- Atualizada a dependência de `Lumia.Foundation.Core` para a versão `0.12.0`.

### 0.20.0

- Adicionada implementação do método GetCurrentUserId a classe BaseApiController

### 0.19.0

- `AddValidationFilters` mudou para o namespace `LumiaFoundation.AspNetCore.Commons.Extensions`

### 0.17.0

- Movidas funcionalidades de autenticação para o novo pacote `Lumia.Foundation.Auth`
- Removidas classes: `RetrieveUserIdFromTokenAttribute`, `AppConfigurationParameter`, `IAppConfigurationParameter`
- Removidas extensões: `ConfigureIdentity`, `ConfigureJWT`, `ConfigureIdentityMariaDbDatabase`
- Removidos DTOs e modelos: `UserForRegistrationDto`, `UserForAuthenticationDto`, `TokenDto`, `User`, `IdentityContext`, `AuthenticationService`, `JwtTokenService`
- Recomenda-se usar o pacote `Lumia.Foundation.Auth` para funcionalidades de autenticação

### 0.16.0

- Reorganização de classes dentro dos pacotes

### 0.15.4

- Adicionada a extensão `AddDomainExceptionMappingFilter` para registrar `DomainExceptionMappingFilter` com tempo de vida `Scoped`.

### 0.15.0

- Adicionado `BaseApiController` para converter exceções de domínio em respostas HTTP.
- Centralizado o mapeamento de exceções usando `switch`, com suporte a `EntityNotFoundException` (404), `CommandValidationException` (422) e fallback para HTTP 500.
- Preservadas as exceções que já são `HttpBaseException`.
- Atualizada a dependência de `Lumia.Foundation.Core` para a versão `0.8.0`.

### 0.14.0

- Movida `DomainBaseException` para o pacote `Lumia.Foundation.Core`, removendo a dependência de HTTP das exceções de domínio.
- Movida `CommandValidationException` e `CommandValidator` para o pacote `Lumia.Foundation.Core`.
- Adicionada `HttpBaseException` como base para exceções que representam respostas HTTP.
- Atualizados os handlers para tratar `HttpBaseException` e manter o tratamento genérico para as demais exceções.
- Adicionada a dependência do pacote `Lumia.Foundation.Core` na versão `0.7.0`.

### 0.13.0

- Adicionadas as extensões `ConfigureOpenApi`, `MapOpenApiDocuments`, `MapScalarUi` e `MapOpenApiDevTools` para configurar OpenAPI e Scalar UI.
- `ConfigureOpenApi` recebe o título e a versão do documento como parâmetros.
- `MapOpenApiDevTools` expõe as ferramentas automaticamente apenas no ambiente de desenvolvimento, com opção de controle explícito.

## 0.12.0

- Atualizada a documentação do pacote sobre o uso de `DomainBaseException` em exceções de domínio.
- Incluída a referência à classe `CommandValidationException` para cenários de validação de comando com resposta HTTP 422.
- Mantido o padrão de que cada exceção derivada define seu status HTTP em `StatusCodeValue`.
- Ajustados os exemplos e a descrição do fluxo de tratamento de erros para refletir a implementação atual.

## 0.11.0

- Tornada a classe `DomainBaseException` abstrata, impedindo sua instanciação direta.
- O código HTTP deve ser definido por cada classe derivada através de `StatusCodeValue`.
- Removido o recebimento do código HTTP pelos construtores da exceção base.

Exemplo de definição de uma exceção de domínio:

```csharp
public sealed class UserNotFoundException : DomainBaseException
{
 protected override int StatusCodeValue => StatusCodes.Status404NotFound;
}
```

## 0.10.0

- Especializado o `DomainExceptionHandler` para tratar apenas exceções do tipo `DomainBaseException`.
- Adaptado o `UnhandledExceptionHandler` para tratar exceções que não são `DomainBaseException`, retornando status HTTP 500.
- Handlers que não correspondem ao tipo da exceção retornam `false`, permitindo o processamento pelo próximo handler.

A lib agora ficou com duas possibilidades de tratamento de erros. A primeira, mais moderna, através de service, configurada como:

```csharp
builder.Services.AddExceptionHandler<DomainExceptionHandler>();
builder.Services.AddExceptionHandler<UnhandledExceptionHandler>();
```

e outra é a forma antiga de fazer, através de middleware, configurada através de Extensions:

```csharp
var logger = app.Services.GetRequiredService<ILoggerManager>();
app.ConfigureExceptionHandler(logger);
```

## 0.9.0

- Refatorado o tratamento de exceções HTTP para registrar erros 5xx como erro e respostas 4xx como aviso.

## 0.8.0

- Adicionado método `AddServicesFromAssembly` em `ServiceCollectionExtensions`
- Permite registrar serviços automaticamente de um assembly seguindo a convenção de nomenclatura (Interface: `I{NomeDaClasse}`, Implementação: `NomeDaClasse`)
- Suporta configuração de `ServiceLifetime` (padrão: Scoped)

## 0.7.0

- Migração para .NET 10.0
