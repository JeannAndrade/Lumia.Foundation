namespace LumiaFoundation.Http.Client.Authentication;

/// <summary>Armazena a sessão do consumidor. Substitua a implementação em aplicações persistentes.</summary>
public interface ITokenStore
{
    ValueTask<AuthenticationToken?> GetAsync(CancellationToken cancellationToken = default);
    ValueTask SetAsync(AuthenticationToken token, CancellationToken cancellationToken = default);
    ValueTask ClearAsync(CancellationToken cancellationToken = default);
}
