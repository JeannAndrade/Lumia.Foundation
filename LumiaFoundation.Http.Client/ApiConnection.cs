using System.Net.Http.Json;
using System.Text.Json;
using LumiaFoundation.Abstractions.ErrorModel;
using LumiaFoundation.Http.Client.Exceptions;

namespace LumiaFoundation.Http.Client;

public sealed class ApiConnection(HttpClient httpClient) : IApiConnection
{
    private readonly HttpClient _httpClient = httpClient;
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    public async Task<T?> SendAsync<T>(HttpMethod method, string requestUri, object? body = null, CancellationToken cancellationToken = default)
    {
        using var response = await SendCoreAsync(method, requestUri, body, cancellationToken);

        if (response.Content.Headers.ContentLength is 0)
            return default;

        return await response.Content.ReadFromJsonAsync<T>(JsonOptions, cancellationToken);
    }

    public async Task SendAsync(HttpMethod method, string requestUri, object? body = null, CancellationToken cancellationToken = default)
    {
        using var response = await SendCoreAsync(method, requestUri, body, cancellationToken);
    }

    private async Task<HttpResponseMessage> SendCoreAsync(HttpMethod method, string requestUri, object? body, CancellationToken cancellationToken)
    {
        using var request = new HttpRequestMessage(method, requestUri);

        if (body is not null)
            request.Content = JsonContent.Create(body, options: JsonOptions);

        var response = await _httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, cancellationToken);

        if (response.IsSuccessStatusCode)
            return response;

        try
        {
            await ThrowApiExceptionAsync(response, cancellationToken);
        }
        finally
        {
            response.Dispose();
        }

        throw new InvalidOperationException("Inalcançável: ThrowApiExceptionAsync sempre lança.");
    }

    // ALTERADO: antes montava um switch retornando 5 tipos diferentes de exceção;
    // agora sempre instancia ApiException, variando só StatusCode e Message.
    private static async Task ThrowApiExceptionAsync(HttpResponseMessage response, CancellationToken cancellationToken)
    {
        ErrorDetails? errorDetails = null;

        try
        {
            errorDetails = await response.Content.ReadFromJsonAsync<ErrorDetails>(JsonOptions, cancellationToken);
        }
        catch (JsonException)
        {
            // corpo não veio no formato esperado (ex: proxy devolvendo HTML em vez de JSON)
        }

        var message = errorDetails?.Message
            ?? $"A API retornou {(int)response.StatusCode} sem corpo de erro reconhecível.";

        throw new ApiException(response.StatusCode, message);
    }
}
