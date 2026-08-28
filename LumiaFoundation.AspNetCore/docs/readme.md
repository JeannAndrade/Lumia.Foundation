# Lumia.Foundation.AspNetCore

Biblioteca com classes utilitárias para aplicações ASP.NET Core, incluindo tratamento global de exceções, filtros de ação, registro automático de serviços e componentes de autenticação com ASP.NET Core Identity e JWT.

## Instalação

Instale o pacote no projeto ASP.NET Core:

```bash
dotnet add package Lumia.Foundation.AspNetCore
```

O pacote depende de `Lumia.Foundation.Logger` e utiliza ASP.NET Core, Entity Framework Core 10, Identity, JWT e MariaDB.

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

Crie exceções de domínio herdando de `DomainBaseException` e defina o status HTTP em `StatusCodeValue`:

```csharp
using LumiaFoundation.AspNetCore.Commons.Exceptions;
using Microsoft.AspNetCore.Http;

public sealed class UserNotFoundException : DomainBaseException
{
 public UserNotFoundException()
  : base("User not found.")
 {
 }

 protected override int StatusCodeValue => StatusCodes.Status404NotFound;
}
```

O código HTTP não é passado pelo construtor da classe base. Exceções com status entre 400 e 499 são registradas como aviso; status entre 500 e 599 são registrados como erro.

### Middleware legado

Como alternativa, use o middleware `ConfigureExceptionHandler`:

```csharp
using LumiaFoundation.AspNetCore.Commons.Extensions;
using LumiaFoundation.Logger.Contracts;

var logger = app.Services.GetRequiredService<ILoggerManager>();
app.ConfigureExceptionHandler(logger);
```

Configure apenas uma das abordagens no pipeline.

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

### ID do usuário a partir do JWT

`RetrieveUserIdFromTokenAttribute` lê o token do cabeçalho `Authorization`, extrai o ID do usuário e o disponibiliza em `HttpContext.Items["UserId"]`:

```csharp
using LumiaFoundation.AspNetCore.ActionFilters;

[ServiceFilter(typeof(RetrieveUserIdFromTokenAttribute))]
public IActionResult GetCurrentUser()
{
 var userId = HttpContext.Items["UserId"]?.ToString();
 return Ok(userId);
}
```

O filtro depende de `IJwtTokenService` e `ILoggerManager` registrados na aplicação.

## Identity e JWT

### Configuração do Identity

`ConfigureIdentity` configura `User` e `IdentityRole`, usa `IdentityContext` como store e habilita os provedores padrão de token:

```csharp
using LumiaFoundation.AspNetCore.Auth.Extensions;

builder.Services.ConfigureIdentity();
```

As regras padrão de senha exigem no mínimo 10 caracteres, dígito, letra maiúscula, letra minúscula e caractere não alfanumérico. O e-mail deve ser único.

Configure o banco do Identity com `ConfigureIdentityMariaDbDatabase<T>()`, informando o contexto e o assembly das migrations:

```csharp
using LumiaFoundation.AspNetCore.Auth.Extensions;
using LumiaFoundation.AspNetCore.Auth.Utils;

var dbConfig = new MariaDbConnectionHelper(
 host: "localhost",
 port: "3306",
 user: "app_user",
 password: "app_password",
 database: "my_app",
 majorVersion: 10,
 minorVersion: 11,
 buildVersion: 0);

builder.Services.ConfigureIdentityMariaDbDatabase<IdentityContext>(
 dbConfig,
 migrationAssembly: typeof(IdentityContext).Assembly.GetName().Name!);
```

### Configuração do JWT

Registre `AppConfigurationParameter` como `IAppConfigurationParameter` e configure o JWT com uma instância obtida da configuração da aplicação:

```csharp
using LumiaFoundation.AspNetCore.Auth.Extensions;
using LumiaFoundation.AspNetCore.Commons.Config;

var configuration = new AppConfigurationParameter(builder.Configuration);
builder.Services.AddSingleton<IAppConfigurationParameter>(configuration);
builder.Services.ConfigureJWT(configuration);
```

As configurações são lidas de `JwtSettings:validIssuer`, `JwtSettings:validAudience`, `JwtSettings:expires` e `JWTSECRET`. Em produção, armazene o segredo em variáveis de ambiente ou em um gerenciador de segredos.

O serviço de autenticação disponibiliza registro de usuário, validação de credenciais, criação de token e atualização de refresh token por meio de `IAuthenticationService`.

## DTOs e modelo de usuário

O pacote inclui `UserForRegistrationDto`, `UserForAuthenticationDto`, `TokenDto` e `User`. O modelo `User` estende `IdentityUser` com nome, sobrenome e dados de refresh token.

## Histórico de versões

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
