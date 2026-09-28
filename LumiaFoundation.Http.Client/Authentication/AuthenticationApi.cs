using System.Net.Http.Json;
using System.Text.Json;
using LumiaFoundation.Http.Client.Exceptions;

namespace LumiaFoundation.Http.Client.Authentication;

internal sealed class AuthenticationApi(HttpClient httpClient, AuthenticationClientOptions options) : IAuthenticationApi
{
    private const string MissingTokensMessage = "A API de autenticação retornou uma resposta sem tokens.";
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
            var token = await response.Content.ReadFromJsonAsync<AuthenticationToken>(JsonOptions, cancellationToken);

            if (token is null
                || string.IsNullOrWhiteSpace(token.AccessToken)
                || string.IsNullOrWhiteSpace(token.RefreshToken))
                throw new ApiException(response.StatusCode, MissingTokensMessage);

            return token;
        }
        catch (JsonException)
        {
            throw new ApiException(response.StatusCode, MissingTokensMessage);
        }
    }
}
