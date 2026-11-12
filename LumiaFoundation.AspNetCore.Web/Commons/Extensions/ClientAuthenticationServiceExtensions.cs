using LumiaFoundation.AspNetCore.ClientAuthentication;
using LumiaFoundation.Http.Client.Authentication;
using Microsoft.Extensions.DependencyInjection;

namespace LumiaFoundation.AspNetCore.Commons.Extensions;

public static class ClientAuthenticationServiceExtensions
{
    public static void AddApiSignInService(this IServiceCollection services)
    {
        services.AddScoped<IApiSignInService, ApiSignInService>();
        services.AddSingleton<ITokenStore, SessionTokenStore>();
    }

    public static void AddApiRegistrationService(this IServiceCollection services)
    {
        services.AddScoped<IApiRegistrationService, ApiRegistrationService>();
    }
}
