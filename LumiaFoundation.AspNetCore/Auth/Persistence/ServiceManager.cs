using LumiaFoundation.AspNetCore.Auth.Identity.Model;
using LumiaFoundation.AspNetCore.Auth.Services;
using LumiaFoundation.AspNetCore.Commons.Config;
using Microsoft.AspNetCore.Identity;

namespace LumiaFoundation.AspNetCore.Auth.Persistence
{
    public sealed class ServiceManager : IServiceManager
    {
        private readonly Lazy<IAuthenticationService> _authenticationService;

        public ServiceManager(UserManager<User> userManager, IAppConfigurationParameter configuration)
        {
            _authenticationService = new Lazy<IAuthenticationService>(() => new AuthenticationService(userManager, configuration));
        }

        public IAuthenticationService AuthenticationService => _authenticationService.Value;
    }
}