# Lumia.Foundation.Auth

Biblioteca com classes utilitárias para autenticação em aplicações ASP.NET Core, incluindo suporte a ASP.NET Core Identity, JWT (JSON Web Tokens), configuração de usuários e gerenciamento de tokens de atualização.

## Requisitos

- **.NET 10.0** (TargetFramework do pacote)
- ASP.NET Core Identity
- Entity Framework Core 10
- MariaDB, MySQL ou PostgreSQL (ou qualquer provider compatível com EF Core)

## Instalação

```bash
dotnet add package Lumia.Foundation.Auth
```

O pacote depende de `Lumia.Foundation.Logger`.

---

## Referência de projeto modelo usando a lib

[CodeMaze](https://github.com/JeannAndrade/CodeMaze)

## Configuração

### 1. Identity

`ConfigureIdentity` registra a classe `User` (que estende `IdentityUser`) com `IdentityRole`, usando `IdentityContext` como store de dados. Habilita os provedores padrão de token para reset de senha e autenticação de dois fatores.

```csharp
using LumiaFoundation.Auth.Extensions;

builder.Services.ConfigureIdentity();
```

**Regras padrão de senha:**

- Comprimento mínimo: **10**
- Requer dígito, letra maiúscula, letra minúscula e caractere não alfanumérico
- E-mail único obrigatório

### 2. Contexto de Identidade

Registre o `IdentityContext` no container de DI apontando para seu banco:

```csharp
public static IServiceCollection ConfigureIdentityDatabase(this IServiceCollection services, IConfiguration configuration)
{
    services.ConfigureMySqlContext<CodeMazeIdentityDbContext>(configuration);
    services.AddScoped<IdentityContext>(provider =>
        provider.GetRequiredService<CodeMazeIdentityDbContext>());

    return services;
}
```

> **Importante:** `IdentityContext` precisa ser herdado no projeto consumidor e o método `OnModelCreating` deve ser sobrescrito chamando `base.OnModelCreating(modelBuilder)`. Aproveite para aplicar configurações como `RoleConfiguration`:
>
> ```csharp
> protected override void OnModelCreating(ModelBuilder modelBuilder)
> {
>     base.OnModelCreating(modelBuilder);
>     modelBuilder.ApplyConfiguration(new RoleConfiguration());
> }
> ```

### 3. JWT (JSON Web Tokens)

Registre `AppConfigurationParameter` como `IAppConfigurationParameter` e configure o JWT:

```csharp
using LumiaFoundation.Auth.Extensions;
using LumiaFoundation.Auth.Config;

var appConfig = new AppConfigurationParameter(builder.Configuration);
builder.Services.ConfigureAppSettingsReader(appConfig);
builder.Services.ConfigureJWT(appConfig);
builder.Services.AddAuthentication();
```

**Configurações lidas:**

| Chave | Descrição | Padrão |
| ------- | ----------- | -------- |
| `JwtSettings:validIssuer` | Emissor do token | `LumiaSoftwareAPI` |
| `JwtSettings:validAudience` | Público-alvo do token | `https://localhost:5001` |
| `JwtSettings:expires` | Expiração em minutos | — |
| `JWT_SECRET` | Chave secreta (use variável de ambiente em produção) | `LumiaSoftwareSecretKey113211162023!!!!` |

Exemplo de `appsettings.json`:

```json
{
  "JwtSettings": {
    "validIssuer": "https://example.com",
    "validAudience": "https://example.com",
    "expires": 15
  }
}
```

> ⚠️ Em produção, armazene o segredo em `JWT_SECRET` (variável de ambiente) ou em um gerenciador de segredos (Azure Key Vault, AWS Secrets Manager, etc.).

### 4. Service Manager

Registre o `ServiceManager` para obter acesso ao `IAuthenticationService`:

```csharp
builder.Services.ConfigureIdentityServiceManager();
```

---

## Serviços

### IAuthenticationService

Interface para operações de autenticação:

| Método | Descrição |
| -------- | ----------- |
| `RegisterUser(UserForRegistrationDto)` | Registra um novo usuário. Valida as roles informadas contra as existentes no banco e as associa ao usuário. |
| `ValidateUser(UserForAuthenticationDto)` | Valida credenciais (usuário + senha). |
| `CreateToken(bool populateExp)` | Gera access token + refresh token. Quando `populateExp = true`, define expiração do refresh token para **7 dias**. |
| `RefreshToken(TokenDto)` | Renova um access token expirado usando o refresh token. Valida assinatura, expiração e correspondência do refresh token armazenado. |

### IJwtTokenService / JwtTokenService

Serviço utilitário para validação de tokens em filtros e middlewares:

| Método | Descrição |
|--------|-----------|
| `ValidateAndDecodeToken(string jwtToken)` | Valida assinatura, issuer, audience e lifetime, retornando o `ClaimsPrincipal`. |
| `GetUserIdFromToken(string jwtToken)` | Retorna o `NameIdentifier` (ID do usuário) do token. Lança `ArgumentException` se não encontrar. |

### IServiceManager / ServiceManager

Ponto central de acesso aos serviços:

```csharp
var serviceManager = app.Services.GetRequiredService<IServiceManager>();
var authService = serviceManager.AuthenticationService;
```

---

## DTOs

### UserForRegistrationDto

```csharp
public class UserForRegistrationDto
{
    public required string FirstName { get; init; }
    public required string LastName { get; init; }
    public required string UserName { get; init; }
    public required string Password { get; init; }
    public string? Email { get; init; }
    public string? PhoneNumber { get; init; }
    public ICollection<string>? Roles { get; init; }

    public User ConvertToUser();
}
```

> `Roles` é opcional. Roles inexistentes são ignoradas e registradas via `ILoggerManager` (warning).

### UserForAuthenticationDto

```csharp
public class UserForAuthenticationDto
{
    public required string UserName { get; init; }
    public required string Password { get; init; }
}
```

### TokenDto

`TokenDto` é um **record** imutável:

```csharp
public record TokenDto(string AccessToken, string RefreshToken);
```

---

## Modelo de Usuário

A classe `User` estende `IdentityUser` com campos adicionais:

```csharp
public class User : IdentityUser
{
    public required string FirstName { get; set; }
    public required string LastName { get; set; }
    public string? RefreshToken { get; set; }
    public DateTime RefreshTokenExpiryTime { get; set; }
}
```

> `FirstName` e `LastName` são `required`. `RefreshToken` é opcional.

---

## Filtros de Ação

### RetrieveUserIdFromTokenAttribute

Extrai o ID do usuário do header `Authorization` e o disponibiliza em `HttpContext.Items["UserId"]`:

```csharp
using LumiaFoundation.Auth.ActionFilters;

[ServiceFilter(typeof(RetrieveUserIdFromTokenAttribute))]
public IActionResult GetCurrentUser()
{
    var userId = HttpContext.Items["UserId"]?.ToString();
    return Ok(new { UserId = userId });
}
```

> O filtro espera o header no formato `Authorization: Bearer <token>`. Também registra o `userId` via `ILoggerManager`.

---

## Exceções

### BadRequestException

Classe **abstrata** base para exceções de requisição inválida.

```csharp
public abstract class BadRequestException : Exception
{
    protected BadRequestException(string message) : base(message) { }
}
```

### RefreshTokenBadRequest

Exceção `sealed` lançada quando o refresh token é inválido, expirado ou não corresponde ao usuário.

```csharp
public sealed class RefreshTokenBadRequest : BadRequestException
{
    public RefreshTokenBadRequest()
        : base("Invalid client request. The tokenDto has some invalid values.") { }
}
```

---

## Configuração (`IAppConfigurationParameter`)

Interface que expõe `JwtParameters`:

```csharp
public interface IAppConfigurationParameter
{
    AppConfigurationParameter.JwtParameters JwtParameter { get; }
}
```

`JwtParameters` fornece:

| Propriedade | Descrição |
| ------------- | ----------- |
| `JwtValidIssuer` | Issuer do token |
| `JwtValidAudience` | Audience do token |
| `JwtSecret` | Chave secreta |
| `JwtExpiresMin` | Expiração em minutos |

---

## Roles Padrão

A classe `RoleConfiguration` semeia duas roles no banco:

- `Manager` (`MANAGER`)
- `Administrator` (`ADMINISTRATOR`)

Aplique-a no `OnModelCreating` do seu contexto derivado de `IdentityContext`.

---

## Exemplo de Uso Completo

```csharp
// Program.cs
var appConfig = new AppConfigurationParameter(builder.Configuration);

builder.Services.AddDbContext<IdentityContext>(opt =>
    opt.UseNpgsql(builder.Configuration.GetConnectionString("Default")));
builder.Services.ConfigureIdentity();
builder.Services.ConfigureAppSettingsReader(appConfig);
builder.Services.ConfigureJWT(appConfig);
builder.Services.ConfigureIdentityServiceManager();
builder.Services.AddScoped<RetrieveUserIdFromTokenAttribute>();
builder.Services.AddScoped<IJwtTokenService, JwtTokenService>();
builder.Services.AddAuthentication();

var app = builder.Build();
app.UseAuthentication();
app.UseAuthorization();
app.Run();
```

---

## Histórico de versões

### 0.2.0

- Revisão da geração e refresh do token

### 0.1.2

- Atualização das dependências internas do projeto para garantir compatibilidade e segurança

### 0.1.0

- Versão inicial do projeto `Lumia.Foundation.Auth`
- Suporte a ASP.NET Core Identity com configuração de regras de senha
- Implementação de autenticação com JWT (JSON Web Tokens)
- Gerenciamento de refresh tokens para renovação de tokens expirados
- Serviços de autenticação (`IAuthenticationService`)
- Filtros de ação para extrair ID do usuário do token JWT
- Contexto de persistência (`IdentityContext`) com Entity Framework Core
- DTOs para registro, autenticação e tokens
- Modelo de usuário estendido com suporte a refresh tokens
