namespace LumiaFoundation.Http.Client.Authentication;

/// <summary>Store de sessão em memória, adequado para aplicações de processo único.</summary>
public sealed class InMemoryTokenStore : ITokenStore
{
    private AuthenticationToken? _token;

    public ValueTask<AuthenticationToken?> GetAsync(CancellationToken cancellationToken = default)
        => ValueTask.FromResult(_token);

    public ValueTask SetAsync(AuthenticationToken token, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(token);
        _token = token;
        return ValueTask.CompletedTask;
    }

    public ValueTask ClearAsync(CancellationToken cancellationToken = default)
    {
        _token = null;
        return ValueTask.CompletedTask;
    }
}
