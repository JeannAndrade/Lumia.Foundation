namespace LumiaFoundation.Http.Client.Authentication;

public interface IAuthenticationApi
{
    Task<AuthenticationToken> LoginAsync(UserCredentials credentials, CancellationToken cancellationToken = default);
    Task<AuthenticationToken> RefreshAsync(AuthenticationToken token, CancellationToken cancellationToken = default);
    Task RegisterAsync(UserRegistration registration, CancellationToken cancellationToken = default);
}
