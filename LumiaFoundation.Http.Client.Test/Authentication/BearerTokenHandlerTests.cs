using System.Net;
using LumiaFoundation.Http.Client.Authentication;
using LumiaFoundation.Http.Client.Test.TestDoubles;

namespace LumiaFoundation.Http.Client.Test.Authentication;

public class BearerTokenHandlerTests
{
    [Fact]
    public async Task SendAsync_WhenThereIsNoSession_ForwardsRequestWithoutAuthorization()
    {
        var tokenStore = new InMemoryTokenStore();
        var authenticationApi = new FakeAuthenticationApi(null);
        var handler = new StubHttpMessageHandler((_, _) => new HttpResponseMessage(HttpStatusCode.OK));
        using var client = new HttpClient(new BearerTokenHandler(tokenStore, authenticationApi)
        {
            InnerHandler = handler
        })
        { BaseAddress = new Uri("https://api.lumia.test/") };

        using var response = await client.GetAsync("public-resource");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Null(handler.LastRequest!.Headers.Authorization);
        Assert.Equal(0, authenticationApi.RefreshCalls);
    }

    [Fact]
    public async Task SendAsync_WhenAccessTokenIsAccepted_ForwardsBearerWithoutRefreshing()
    {
        var tokenStore = new InMemoryTokenStore();
        await tokenStore.SetAsync(new AuthenticationToken("valid-access", "refresh-token"));
        var authenticationApi = new FakeAuthenticationApi(null);
        var handler = new StubHttpMessageHandler((_, _) => new HttpResponseMessage(HttpStatusCode.OK));
        using var client = new HttpClient(new BearerTokenHandler(tokenStore, authenticationApi)
        {
            InnerHandler = handler
        })
        { BaseAddress = new Uri("https://api.lumia.test/") };

        using var response = await client.GetAsync("protected-resource");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("Bearer", handler.LastRequest!.Headers.Authorization!.Scheme);
        Assert.Equal("valid-access", handler.LastRequest.Headers.Authorization.Parameter);
        Assert.Equal(0, authenticationApi.RefreshCalls);
    }

    [Fact]
    public async Task SendAsync_WhenAccessTokenIsRejected_RefreshesAndRetriesOnce()
    {
        var initial = new AuthenticationToken("expired-access", "refresh-token");
        var refreshed = new AuthenticationToken("new-access", "new-refresh-token");
        var tokenStore = new InMemoryTokenStore();
        await tokenStore.SetAsync(initial);
        var authenticationApi = new FakeAuthenticationApi(refreshed);
        var handler = new StubHttpMessageHandler((request, _) =>
            request.Headers.Authorization?.Parameter == "expired-access"
                ? new HttpResponseMessage(HttpStatusCode.Unauthorized)
                : new HttpResponseMessage(HttpStatusCode.OK));
        using var client = new HttpClient(new BearerTokenHandler(tokenStore, authenticationApi)
        {
            InnerHandler = handler
        })
        { BaseAddress = new Uri("https://api.lumia.test/") };

        using var response = await client.GetAsync("protected-resource");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal(1, authenticationApi.RefreshCalls);
        Assert.Equal(refreshed, await tokenStore.GetAsync());
    }

    [Fact]
    public async Task SendAsync_WhenRefreshFails_KeepsUnauthorizedResponseAndClearsSession()
    {
        var tokenStore = new InMemoryTokenStore();
        await tokenStore.SetAsync(new AuthenticationToken("expired-access", "refresh-token"));
        var authenticationApi = new FakeAuthenticationApi(null);
        var handler = new StubHttpMessageHandler((_, _) => new HttpResponseMessage(HttpStatusCode.Unauthorized));
        using var client = new HttpClient(new BearerTokenHandler(tokenStore, authenticationApi)
        {
            InnerHandler = handler
        })
        { BaseAddress = new Uri("https://api.lumia.test/") };

        using var response = await client.GetAsync("protected-resource");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        Assert.Equal(1, authenticationApi.RefreshCalls);
        Assert.Null(await tokenStore.GetAsync());
    }

    [Fact]
    public async Task SendAsync_WhenRequestHasContent_ClonesAndRefreshesWithContent()
    {
        var initial = new AuthenticationToken("expired-access", "refresh-token");
        var refreshed = new AuthenticationToken("new-access", "new-refresh-token");
        var tokenStore = new InMemoryTokenStore();
        await tokenStore.SetAsync(initial);
        var authenticationApi = new FakeAuthenticationApi(refreshed);
        var handler = new StubHttpMessageHandler((request, _) =>
            request.Headers.Authorization?.Parameter == "expired-access"
                ? new HttpResponseMessage(HttpStatusCode.Unauthorized)
                : new HttpResponseMessage(HttpStatusCode.OK));
        using var client = new HttpClient(new BearerTokenHandler(tokenStore, authenticationApi)
        {
            InnerHandler = handler
        })
        { BaseAddress = new Uri("https://api.lumia.test/") };

        using var response = await client.PostAsync("protected-resource",
            new StringContent("{\"key\":\"value\"}", System.Text.Encoding.UTF8, "application/json"));

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal(1, authenticationApi.RefreshCalls);
    }


    private sealed class FakeAuthenticationApi(AuthenticationToken? refreshResult) : IAuthenticationApi
    {
        public int RefreshCalls { get; private set; }

        public Task<AuthenticationToken> LoginAsync(UserCredentials credentials, CancellationToken cancellationToken = default)
            => throw new NotSupportedException();

        public Task<AuthenticationToken> RefreshAsync(AuthenticationToken token, CancellationToken cancellationToken = default)
        {
            RefreshCalls++;
            return refreshResult is null
                ? Task.FromException<AuthenticationToken>(new InvalidOperationException("Refresh inválido."))
                : Task.FromResult(refreshResult);
        }
    }
}
