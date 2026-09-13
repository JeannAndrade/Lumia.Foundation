# Lumia.Foundation.Http.Client

Cliente HTTP tipado para consumo de APIs, com registro no `HttpClientFactory`, serialização JSON, conversão de respostas de erro em `ApiException` e autenticação JWT com refresh automático.

## Instalação

Instale o pacote no projeto que consumirá a API:

```bash
dotnet add package Lumia.Foundation.Http.Client
```

## Registro do cliente

Registre o cliente informando o endereço base da API:

```csharp
using LumiaFoundation.Http.Client;

builder.Services.AddLumiaApiClient(
    baseAddress: "https://api.example.com");
```

O método registra `IApiConnection`/`ApiConnection` como um cliente nomeado pelo `HttpClientFactory`. Para configurar cabeçalhos ou outras opções do `HttpClient`, use o parâmetro `configureClient`:

```csharp
builder.Services.AddLumiaApiClient(
    "https://api.example.com",
    client =>
    {
        client.DefaultRequestHeaders.Add("X-Application", "Orders");
    });
```

Depois, injete `IApiConnection` no serviço que fará as chamadas:

```csharp
using LumiaFoundation.Http.Client;

public sealed class OrderService(IApiConnection apiConnection)
{
    public Task<OrderDto?> GetAsync(
        int orderId,
        CancellationToken cancellationToken = default)
    {
        return apiConnection.SendAsync<OrderDto>(
            HttpMethod.Get,
            $"/orders/{orderId}",
            ct: cancellationToken);
    }
}
```

## Enviando requisições

`SendAsync<T>` envia a requisição e desserializa a resposta JSON para o tipo informado. O corpo opcional também é serializado como JSON:

```csharp
var order = await apiConnection.SendAsync<OrderDto>(
    HttpMethod.Post,
    "/orders",
    new CreateOrderRequest("customer-123"),
    cancellationToken);
```

Para chamadas que não retornam conteúdo, use a sobrecarga sem tipo:

```csharp
await apiConnection.SendAsync(
    HttpMethod.Delete,
    $"/orders/{orderId}",
    ct: cancellationToken);
```

As URLs podem ser relativas ao `BaseAddress` configurado no registro ou absolutas. O `CancellationToken` é encaminhado para o envio e para a leitura da resposta.

Quando a resposta não possui conteúdo (`Content-Length` igual a zero), `SendAsync<T>` retorna o valor padrão de `T`. As respostas bem-sucedidas usam as opções JSON do perfil Web (`JsonSerializerDefaults.Web`), incluindo os nomes de propriedades esperados por APIs HTTP comuns.

## Tratamento de erros

Respostas HTTP fora da faixa de sucesso lançam `ApiException`. A exceção mantém o status HTTP em `StatusCode` e a mensagem retornada pela API quando o corpo contém um `ErrorDetails` reconhecível:

```csharp
using System.Net;
using LumiaFoundation.Http.Client.Exceptions;

try
{
    await apiConnection.SendAsync(
        HttpMethod.Put,
        $"/orders/{orderId}",
        updateRequest,
        cancellationToken);
}
catch (ApiException exception)
{
    if (exception.StatusCode == HttpStatusCode.NotFound)
    {
        // Trate o recurso inexistente.
    }

    // Registre ou converta a mensagem conforme o contrato da aplicação.
}
```

Quando o corpo de erro não está no formato `ErrorDetails`, a mensagem da exceção informa o código HTTP e indica que não foi possível reconhecer o corpo (`"A API retornou {status} sem corpo de erro reconhecível."`). Falhas de transporte e cancelamentos continuam sendo propagados pelo `HttpClient`.

> **Nota:** a partir da versão 0.2.0, `ApiException` é a única exceção lançada para respostas HTTP não-sucedidas. Não há mais subtipos como `NotFoundException`, `BadRequestException` etc.

## Autenticação JWT

O cliente já vem preparado para APIs que usam `Lumia.Foundation.Auth`. Ele mantém os tokens em `ITokenStore`, acrescenta `Authorization: Bearer <accessToken>` às chamadas de `IApiConnection` e, ao receber um único `401`, chama o endpoint de refresh, atualiza a sessão e repete a chamada uma vez.

```csharp
using LumiaFoundation.Http.Client.Authentication;

public sealed class SessionService(
    IAuthenticationApi authentication,
    ITokenStore tokenStore)
{
    public async Task LoginAsync(string userName, string password)
    {
        var tokens = await authentication.LoginAsync(new UserCredentials(userName, password));
        await tokenStore.SetAsync(tokens);
    }
}
```

As rotas padrão são `/api/authentication` e `/api/authentication/refresh`, compatíveis com o exemplo CodeMaze. Elas podem ser alteradas no registro:

```csharp
builder.Services.AddLumiaApiClient(
    "https://api.example.com",
    configureAuthentication: options =>
    {
        options.LoginPath = "/auth/login";
        options.RefreshPath = "/auth/refresh";
    });
```

### Como o refresh funciona

O `BearerTokenHandler` é um `DelegatingHandler` registrado **apenas** no cliente de `IApiConnection` (não no `IAuthenticationApi`, evitando loop de refresh). O fluxo é:

1. Lê o token atual de `ITokenStore`.
2. Se não houver token, encaminha a requisição sem `Authorization`.
3. Se houver, anexa `Authorization: Bearer <accessToken>` e envia.
4. Se a resposta for `401 Unauthorized`:
   - Adquire um `SemaphoreSlim` para serializar refreshes concorrentes.
   - Relê o token (outra thread pode ter refrescado).
   - Chama `IAuthenticationApi.RefreshAsync`.
   - Em caso de falha, limpa o `ITokenStore` e devolve o `401` original.
   - Em caso de sucesso, persiste o novo token e repete a chamada uma única vez.
5. Se a resposta não for `401`, devolve-a ao chamador.

O corpo da requisição é clonado antes do envio original para permitir o replay.

### Armazenamento de tokens

`InMemoryTokenStore` serve a processos únicos (registrado como `Singleton`). Aplicações web, desktop ou móveis devem substituir `ITokenStore` por uma implementação adequada ao seu mecanismo seguro de sessão (cofre, keychain, storage criptografado etc.):

```csharp
public sealed class KeychainTokenStore : ITokenStore
{
    public ValueTask<AuthenticationToken?> GetAsync(CancellationToken ct = default)
        => /* ... */;

    public ValueTask SetAsync(AuthenticationToken token, CancellationToken ct = default)
        => /* ... */;

    public ValueTask ClearAsync(CancellationToken ct = default)
        => /* ... */;
}
```

## Clientes de recursos

Para registrar uma implementação específica de um recurso na injeção de dependência, use `AddApiResourceClient<TInterface, TImplementation>`:

```csharp
public interface IOrderApi
{
    Task<OrderDto?> GetAsync(
        int orderId,
        CancellationToken cancellationToken = default);
}

public sealed class OrderApi(IApiConnection apiConnection) : IOrderApi
{
    public Task<OrderDto?> GetAsync(
        int orderId,
        CancellationToken cancellationToken = default)
    {
        return apiConnection.SendAsync<OrderDto>(
            HttpMethod.Get,
            $"/orders/{orderId}",
            ct: cancellationToken);
    }
}
```

Registre o recurso como `Transient`:

```csharp
builder.Services.AddApiResourceClient<IOrderApi, OrderApi>();
```

> **Importante:** `AddApiResourceClient` **não** cria um `HttpClient` próprio — ele apenas registra `TInterface` → `TImplementation` como `Transient`. O `IApiConnection` injetado no recurso continua sendo aquele configurado por `AddLumiaApiClient`.

## Componentes disponíveis

| Componente | Finalidade |
| :--- | :--- |
| `IApiConnection` | Abstração para enviar requisições HTTP com ou sem retorno tipado. |
| `ApiConnection` | Implementação que serializa corpos e desserializa respostas JSON. |
| `ApiException` | Exceção para respostas HTTP que não indicam sucesso. Mantém `StatusCode` e a mensagem da API. |
| `AddLumiaApiClient` | Registra o cliente HTTP, o `BaseAddress`, o `BearerTokenHandler`, o `ITokenStore` e o `IAuthenticationApi`. |
| `AddApiResourceClient<TInterface, TImplementation>` | Registra um cliente de recurso como `Transient`. |
| `IAuthenticationApi` | Abstração para `LoginAsync` e `RefreshAsync`. |
| `AuthenticationApi` | Implementação que chama os endpoints configurados em `AuthenticationClientOptions`. |
| `AuthenticationClientOptions` | Rotas de login e refresh (`LoginPath`, `RefreshPath`). |
| `AuthenticationToken` | `record` com `AccessToken` e `RefreshToken`. |
| `UserCredentials` | `record` com `UserName` e `Password`. |
| `ITokenStore` | Armazena a sessão do consumidor (`GetAsync`, `SetAsync`, `ClearAsync`). |
| `InMemoryTokenStore` | Implementação em memória, adequada para processos únicos. |
| `BearerTokenHandler` | `DelegatingHandler` que anexa o token e renova em um único `401`. |

## Histórico de versões

### 0.2.0

- Adicionada autenticação JWT com `IAuthenticationApi`, `AuthenticationToken`, `UserCredentials` e `AuthenticationClientOptions`.
- Adicionado `ITokenStore` com implementação padrão `InMemoryTokenStore`.
- Adicionado `BearerTokenHandler`, que anexa `Authorization: Bearer` e renova a sessão em um único `401`, com `SemaphoreSlim` para evitar refreshes concorrentes.
- `AddLumiaApiClient` agora aceita `configureAuthentication` e registra os serviços de autenticação automaticamente.
- `ApiException` passa a ser a única exceção lançada para respostas HTTP não-sucedidas (substitui os subtipos anteriores).

### 0.1.0

- Versão inicial do cliente HTTP para consumo de APIs.
- Adicionados os métodos `AddLumiaApiClient` e `AddApiResourceClient<TInterface, TImplementation>`.
- Adicionado suporte a requisições com corpo JSON, respostas tipadas e `ApiException`.
