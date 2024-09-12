using LumiaFoundation.AspNetCore.Auth.Identity.Model;
using LumiaFoundation.AspNetCore.Auth.Services;
using LumiaFoundation.AspNetCore.Commons.Config;
using LumiaFoundation.Logger.Contracts;
using Microsoft.AspNetCore.Identity;

namespace LumiaFoundation.AspNetCore.Auth.Persistence
{
    public sealed class ServiceManager(UserManager<User> userManager, IAppConfigurationParameter configuration, ILoggerManager logger) : IServiceManager
    {
        private readonly Lazy<IAuthenticationService> _authenticationService = new Lazy<IAuthenticationService>(() => new AuthenticationService(userManager, configuration, logger));

        public IAuthenticationService AuthenticationService => _authenticationService.Value;
    }
}