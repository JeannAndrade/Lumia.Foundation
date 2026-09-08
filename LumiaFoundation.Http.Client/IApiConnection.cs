namespace LumiaFoundation.Http.Client;

public interface IApiConnection
{
  Task<T?> SendAsync<T>(HttpMethod method, string requestUri, object? body = null, CancellationToken ct = default);
  Task SendAsync(HttpMethod method, string requestUri, object? body = null, CancellationToken ct = default); // sem retorno
}
