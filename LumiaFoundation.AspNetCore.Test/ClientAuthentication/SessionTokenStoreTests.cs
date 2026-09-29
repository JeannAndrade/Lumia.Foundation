using LumiaFoundation.AspNetCore.ClientAuthentication;
using LumiaFoundation.Http.Client.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;

namespace LumiaFoundation.AspNetCore.Test.ClientAuthentication;

public class SessionTokenStoreTests
{
    private static readonly IOptions<SessionTokenStoreOptions> DefaultOptions =
        Options.Create(new SessionTokenStoreOptions());

    private static HttpContextAccessor CreateAccessor(FakeSession? session = null) =>
        new() { HttpContext = new DefaultHttpContext { Session = session ?? new FakeSession() } };

    [Fact]
    public async Task GetAsync_WhenSessionHasNoToken_ReturnsNull()
    {
        var store = new SessionTokenStore(CreateAccessor(), DefaultOptions);

        var result = await store.GetAsync();

        Assert.Null(result);
    }

    [Fact]
    public async Task SetAsync_ThenGetAsync_ReturnsSameToken()
    {
        var store = new SessionTokenStore(CreateAccessor(), DefaultOptions);
        var token = new AuthenticationToken("access", "refresh");

        await store.SetAsync(token);
        var result = await store.GetAsync();

        Assert.Equal(token, result);
    }

    [Fact]
    public async Task SetAsync_CommitsTheSession()
    {
        var session = new FakeSession();
        var store = new SessionTokenStore(CreateAccessor(session), DefaultOptions);

        await store.SetAsync(new AuthenticationToken("access", "refresh"));

        Assert.Equal(1, session.CommitCount);
    }

    [Fact]
    public async Task ClearAsync_RemovesTheToken()
    {
        var store = new SessionTokenStore(CreateAccessor(), DefaultOptions);
        await store.SetAsync(new AuthenticationToken("access", "refresh"));

        await store.ClearAsync();

        Assert.Null(await store.GetAsync());
    }

    [Fact]
    public async Task DifferentSessionKeys_DoNotCollideInTheSameSession()
    {
        // Prova o motivo de existir a SessionTokenStoreOptions: dois stores, duas chaves,
        // mesma sessão (cenário de futuras múltiplas APIs).
        var session = new FakeSession();
        var accessor = CreateAccessor(session);
        var vaultStore = new SessionTokenStore(accessor, Options.Create(new SessionTokenStoreOptions { SessionKey = "Vault" }));
        var tesouroStore = new SessionTokenStore(accessor, Options.Create(new SessionTokenStoreOptions { SessionKey = "Tesouro" }));

        await vaultStore.SetAsync(new AuthenticationToken("vault-access", "vault-refresh"));
        await tesouroStore.SetAsync(new AuthenticationToken("tesouro-access", "tesouro-refresh"));

        Assert.Equal("vault-access", (await vaultStore.GetAsync())!.AccessToken);
        Assert.Equal("tesouro-access", (await tesouroStore.GetAsync())!.AccessToken);
    }

    [Fact]
    public async Task SameStoreInstance_WhenRequestChanges_IsolatesTokensBetweenUsers()
    {
        // Reproduz o cenário do HttpClientFactory: uma única instância do store atendendo
        // requisições de usuários diferentes.
        var accessor = CreateAccessor();
        var store = new SessionTokenStore(accessor, DefaultOptions);
        var tokenA = new AuthenticationToken("access-A", "refresh-A");
        var tokenB = new AuthenticationToken("access-B", "refresh-B");

        var contextA = accessor.HttpContext!;
        await store.SetAsync(tokenA);

        accessor.HttpContext = new DefaultHttpContext { Session = new FakeSession() };
        var visibleToB = await store.GetAsync();
        await store.SetAsync(tokenB);

        accessor.HttpContext = contextA;
        var visibleToA = await store.GetAsync();

        Assert.Null(visibleToB);
        Assert.Equal(tokenA, visibleToA);
    }

    [Fact]
    public async Task GetAsync_WhenThereIsNoHttpContext_ThrowsInvalidOperationException()
    {
        var store = new SessionTokenStore(new HttpContextAccessor(), DefaultOptions);

        await Assert.ThrowsAsync<InvalidOperationException>(async () => await store.GetAsync());
    }
}
