using System.Text.Json;
using LumiaFoundation.Http.Client.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;

namespace LumiaFoundation.AspNetCore.ClientAuthentication;

/// <summary>
/// Guarda o par de tokens da API na sessão do usuário atual (<see cref="ISession"/>).
/// </summary>
/// <remarks>
/// Esta classe não guarda estado em campos: a sessão é buscada a cada chamada, a partir da
/// requisição em andamento, via <see cref="IHttpContextAccessor"/>. Isso é o que isola um
/// usuário do outro, já que o <c>HttpClientFactory</c> reutiliza a cadeia de handlers — e o
/// <see cref="ITokenStore"/> que ela recebe — entre requisições de usuários diferentes. Por
/// não ter estado próprio, registre-a como <c>Singleton</c>.
/// </remarks>
public sealed class SessionTokenStore(
    IHttpContextAccessor httpContextAccessor,
    IOptions<SessionTokenStoreOptions> options) : ITokenStore
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    public async ValueTask<AuthenticationToken?> GetAsync(CancellationToken cancellationToken = default)
    {
        var session = GetSession();
        await session.LoadAsync(cancellationToken);

        return session.TryGetValue(options.Value.SessionKey, out var bytes)
            ? JsonSerializer.Deserialize<AuthenticationToken>(bytes, JsonOptions)
            : null;
    }

    public async ValueTask SetAsync(AuthenticationToken token, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(token);

        var session = GetSession();
        await session.LoadAsync(cancellationToken);
        session.Set(options.Value.SessionKey, JsonSerializer.SerializeToUtf8Bytes(token, JsonOptions));
        await session.CommitAsync(cancellationToken);
    }

    public async ValueTask ClearAsync(CancellationToken cancellationToken = default)
    {
        var session = GetSession();
        await session.LoadAsync(cancellationToken);
        session.Remove(options.Value.SessionKey);
        await session.CommitAsync(cancellationToken);
    }

    private ISession GetSession() =>
        httpContextAccessor.HttpContext?.Session
        ?? throw new InvalidOperationException(
            "Não há requisição HTTP com sessão disponível para acessar o token da API.");
}
