using System.Net.Http.Json;
using System.Text.Json;
using LumiaFoundation.Http.Client.Exceptions;

namespace LumiaFoundation.Http.Client.Authentication;

internal sealed class AuthenticationApi(HttpClient httpClient, AuthenticationClientOptions options) : IAuthenticationApi
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    public Task<AuthenticationToken> LoginAsync(UserCredentials credentials, CancellationToken cancellationToken = default)
        => SendAsync(options.LoginPath, credentials, cancellationToken);

    public Task<AuthenticationToken> RefreshAsync(AuthenticationToken token, CancellationToken cancellationToken = default)
        => SendAsync(options.RefreshPath, token, cancellationToken);

    private async Task<AuthenticationToken> SendAsync(string path, object body, CancellationToken cancellationToken)
    {
        using var response = await httpClient.PostAsJsonAsync(path, body, JsonOptions, cancellationToken);
        if (!response.IsSuccessStatusCode)
            throw new ApiException(response.StatusCode, await ApiError.ReadMessageAsync(response, cancellationToken));

        try
        {
            return await response.Content.ReadFromJsonAsync<AuthenticationToken>(JsonOptions, cancellationToken)
                ?? throw new ApiException(response.StatusCode, "A API de autenticação retornou uma resposta sem tokens.");
        }
        catch (JsonException)
        {
            throw new ApiException(response.StatusCode, "A API de autenticação retornou uma resposta sem tokens.");
        }
    }
}
