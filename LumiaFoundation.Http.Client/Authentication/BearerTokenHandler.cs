using System.Net;
using System.Net.Http.Headers;

namespace LumiaFoundation.Http.Client.Authentication;

/// <summary>Anexa o access token e, diante de um único 401, renova a sessão e repete a chamada.</summary>
public sealed class BearerTokenHandler(ITokenStore tokenStore, IAuthenticationApi authenticationApi) : DelegatingHandler
{
    private readonly SemaphoreSlim _refreshLock = new(1, 1);

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var token = await tokenStore.GetAsync(cancellationToken);
        if (token is null)
            return await base.SendAsync(request, cancellationToken);

        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token.AccessToken);
        var retryRequest = await CloneAsync(request, cancellationToken);
        var response = await base.SendAsync(request, cancellationToken);

        if (response.StatusCode != HttpStatusCode.Unauthorized)
        {
            retryRequest.Dispose();
            return response;
        }

        await _refreshLock.WaitAsync(cancellationToken);
        try
        {
            var currentToken = await tokenStore.GetAsync(cancellationToken);
            if (currentToken is null)
            {
                retryRequest.Dispose();
                return response;
            }

            AuthenticationToken refreshedToken;
            try
            {
                refreshedToken = await authenticationApi.RefreshAsync(currentToken, cancellationToken);
            }
            catch
            {
                await tokenStore.ClearAsync(cancellationToken);
                retryRequest.Dispose();
                return response;
            }

            await tokenStore.SetAsync(refreshedToken, cancellationToken);
            response.Dispose();
            retryRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", refreshedToken.AccessToken);
            return await base.SendAsync(retryRequest, cancellationToken);
        }
        finally
        {
            _refreshLock.Release();
        }
    }

    private static async Task<HttpRequestMessage> CloneAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var clone = new HttpRequestMessage(request.Method, request.RequestUri)
        {
            Version = request.Version,
            VersionPolicy = request.VersionPolicy
        };

        foreach (var header in request.Headers)
            clone.Headers.TryAddWithoutValidation(header.Key, header.Value);

        if (request.Content is not null)
        {
            var content = new ByteArrayContent(await request.Content.ReadAsByteArrayAsync(cancellationToken));
            foreach (var header in request.Content.Headers)
                content.Headers.TryAddWithoutValidation(header.Key, header.Value);
            clone.Content = content;
        }

        return clone;
    }
}
