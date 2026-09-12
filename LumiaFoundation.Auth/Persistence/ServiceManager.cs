using LumiaFoundation.Auth.Identity.Model;
using LumiaFoundation.Auth.Services;
using LumiaFoundation.Logger.Contracts;
using Microsoft.AspNetCore.Identity;
using LumiaFoundation.Auth.Config;

namespace LumiaFoundation.Auth.Persistence;

public sealed class ServiceManager(UserManager<User> userManager, RoleManager<IdentityRole> roleManager, IAppConfigurationParameter configuration, ILoggerManager logger) : IServiceManager
{
    private readonly Lazy<IAuthenticationService> _authenticationService =
    new(() => new AuthenticationService(userManager, roleManager, configuration, logger));

    public IAuthenticationService AuthenticationService => _authenticationService.Value;
}
