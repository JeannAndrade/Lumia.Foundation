using LumiaFoundation.AspNetCore.Auth.Services;

namespace LumiaFoundation.AspNetCore.Auth.Persistence
{
    public interface IServiceManager
    {
        IAuthenticationService AuthenticationService { get; }
    }
}