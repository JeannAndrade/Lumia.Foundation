using LumiaFoundation.Auth.Identity.Model;
using LumiaFoundation.Auth.Services;
using LumiaFoundation.Logger.Contracts;
using Microsoft.AspNetCore.Identity;
using LumiaFoundation.Auth.Config;

namespace LumiaFoundation.Auth.Persistence;

public sealed class ServiceManager(UserManager<User> userManager, IAppConfigurationParameter configuration, ILoggerManager logger) : IServiceManager
{
    private readonly Lazy<IAuthenticationService> _authenticationService = new Lazy<IAuthenticationService>(() => new AuthenticationService(userManager, configuration, logger));

    public IAuthenticationService AuthenticationService => _authenticationService.Value;
}
