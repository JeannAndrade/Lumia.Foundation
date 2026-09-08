# Lumia.Foundation.Http.Client

Cliente HTTP tipado para consumo de APIs, com registro no `HttpClientFactory`, serialização JSON e conversão de respostas de erro em `ApiException`.

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

O método registra `IApiConnection` e `ApiConnection` como um cliente nomeado pelo `HttpClientFactory`. Para configurar cabeçalhos ou outras opções do `HttpClient`, use o parâmetro `configureClient`:

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

Quando a resposta não possui conteúdo, `SendAsync<T>` retorna o valor padrão de `T`. As respostas bem-sucedidas usam as opções JSON do perfil Web, incluindo os nomes de propriedades esperados por APIs HTTP comuns.

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

Quando o corpo de erro não está no formato esperado, a exceção informa o código HTTP e indica que não foi possível reconhecer o corpo. Falhas de transporte e cancelamentos continuam sendo propagados pelo `HttpClient`.

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

## Componentes disponíveis

| Componente | Finalidade |
| :--- | :--- |
| `IApiConnection` | Abstração para enviar requisições HTTP com ou sem retorno tipado. |
| `ApiConnection` | Implementação que serializa corpos e desserializa respostas JSON. |
| `AddLumiaApiClient` | Registra o cliente HTTP e configura o `BaseAddress`. |
| `AddApiResourceClient<TInterface, TImplementation>` | Registra um cliente de recurso como `Transient`. |
| `ApiException` | Exceção para respostas HTTP que não indicam sucesso. |

## Histórico de versões

### 0.1.0

- Versão inicial do cliente HTTP para consumo de APIs.
- Adicionados os métodos `AddLumiaApiClient` e `AddApiResourceClient<TInterface, TImplementation>`.
- Adicionado suporte a requisições com corpo JSON, respostas tipadas e `ApiException`.
