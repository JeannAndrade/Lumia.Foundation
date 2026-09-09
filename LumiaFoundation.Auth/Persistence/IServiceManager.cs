using LumiaFoundation.Auth.Services;

namespace LumiaFoundation.Auth.Persistence;

public interface IServiceManager
{
    IAuthenticationService AuthenticationService { get; }
}
