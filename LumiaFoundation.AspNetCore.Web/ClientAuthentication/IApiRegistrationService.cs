using LumiaFoundation.Http.Client.Authentication;

namespace LumiaFoundation.AspNetCore.Web.ClientAuthentication;

public interface IApiRegistrationService
{
    Task<RegistrationResult> RegisterAsync(UserRegistration registration, CancellationToken cancellationToken = default);
}
