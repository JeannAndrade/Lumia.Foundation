# Lumia.Foundation.Auth

Biblioteca com classes utilitárias para autenticação em aplicações ASP.NET Core, incluindo suporte a ASP.NET Core Identity, JWT (JSON Web Tokens), configuração de usuários e gerenciamento de tokens de atualização.

## Instalação

Instale o pacote no projeto ASP.NET Core:

```bash
dotnet add package Lumia.Foundation.Auth
```

O pacote depende de `Lumia.Foundation.Logger` e utiliza ASP.NET Core, Entity Framework Core 10, Identity, JWT e Sql Server como padrão (com suporte a MariaDB e PostgreSQL).

## Configuração

### Identity

`ConfigureIdentity` configura a classe `User` (que estende `IdentityUser`) com `IdentityRole` e usa `IdentityContext` como store de dados. Habilita os provedores padrão de token para reset de senha e autenticação de dois fatores:

```csharp
using LumiaFoundation.Auth.Extensions;

builder.Services.ConfigureIdentity();
```

As regras padrão de senha exigem no mínimo 10 caracteres, dígito, letra maiúscula, letra minúscula e caractere não alfanumérico. O e-mail deve ser único.

### Contexto de Identidade

Configure o banco de dados para persistência de usuários com `IdentityContext`:

```csharp
using LumiaFoundation.Auth.Persistence;
using Microsoft.EntityFrameworkCore;

builder.Services.AddDbContext<IdentityContext>(options =>
    options.UseSqlServer(connectionString));
```

### JWT (JSON Web Tokens)

Registre `AppConfigurationParameter` como `IAppConfigurationParameter` e configure o JWT com uma instância obtida da configuração da aplicação:

```csharp
using LumiaFoundation.Auth.Extensions;
using LumiaFoundation.Auth.Config;

var configuration = new AppConfigurationParameter(builder.Configuration);
builder.Services.AddSingleton<IAppConfigurationParameter>(configuration);
builder.Services.ConfigureJWT(configuration);
builder.Services.AddAuthentication();
```

As configurações são lidas de:

- `JwtSettings:validIssuer` - Emissor do token
- `JwtSettings:validAudience` - Público-alvo do token
- `JwtSettings:expires` - Tempo de expiração em minutos
- `JWTSECRET` - Chave secreta (deve ser uma variável de ambiente em produção)

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

Em produção, armazene o segredo `JWTSECRET` em variáveis de ambiente ou em um gerenciador de segredos como Azure Key Vault.

## Serviços de Autenticação

### IAuthenticationService

Interface para operações de autenticação com os seguintes métodos:

- `RegisterUser(UserForRegistrationDto)` - Registra um novo usuário
- `ValidateUser(UserForAuthenticationDto)` - Valida credenciais do usuário
- `CreateToken(bool populateExp)` - Cria um token JWT e refresh token
- `RefreshToken(TokenDto)` - Atualiza um token expirado usando o refresh token

### IServiceManager

Interface para gerenciamento centralizado de serviços de autenticação:

```csharp
using LumiaFoundation.Auth.Persistence;

var serviceManager = app.Services.GetRequiredService<IServiceManager>();
```

## DTOs (Data Transfer Objects)

### UserForRegistrationDto

Dados necessários para registrar um novo usuário:

```csharp
public class UserForRegistrationDto
{
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string Email { get; set; }
    public string UserName { get; set; }
    public string Password { get; set; }
    public string PhoneNumber { get; set; }
}
```

### UserForAuthenticationDto

Dados necessários para autenticar um usuário:

```csharp
public class UserForAuthenticationDto
{
    public string UserName { get; set; }
    public string Password { get; set; }
}
```

### TokenDto

Dados retornados após autenticação bem-sucedida:

```csharp
public class TokenDto
{
    public string AccessToken { get; set; }
    public string RefreshToken { get; set; }
}
```

## Modelo de Usuário

A classe `User` estende `IdentityUser` com campos adicionais:

```csharp
public class User : IdentityUser
{
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string RefreshToken { get; set; }
    public DateTime RefreshTokenExpiryTime { get; set; }
}
```

## Filtros de Ação

### RetrieveUserIdFromTokenAttribute

Extrai o ID do usuário do token JWT e o disponibiliza em `HttpContext.Items["UserId"]`:

```csharp
using LumiaFoundation.Auth.ActionFilters;

[ServiceFilter(typeof(RetrieveUserIdFromTokenAttribute))]
public IActionResult GetCurrentUser()
{
    var userId = HttpContext.Items["UserId"]?.ToString();
    return Ok(new { UserId = userId });
}
```

## Exceções

### BadRequestException

Exceção lançada quando há erro na requisição de autenticação ou registro.

### RefreshTokenBadRequest

Exceção lançada quando o refresh token é inválido ou expirou.

## Histórico de versões

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
