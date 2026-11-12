using LumiaFoundation.Http.Client.Authentication;

namespace LumiaFoundation.AspNetCore.ClientAuthentication;

public interface IApiRegistrationService
{
    Task<RegistrationResult> RegisterAsync(UserRegistration registration, CancellationToken cancellationToken = default);
}
