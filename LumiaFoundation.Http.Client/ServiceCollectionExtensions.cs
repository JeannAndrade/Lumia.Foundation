using Microsoft.Extensions.DependencyInjection;
using LumiaFoundation.Http.Client.Authentication;

namespace LumiaFoundation.Http.Client;

public static class ServiceCollectionExtensions
{
    public static IHttpClientBuilder AddLumiaApiClient(
        this IServiceCollection services,
        string baseAddress,
        Action<HttpClient>? configureClient = null,
        Action<AuthenticationClientOptions>? configureAuthentication = null)
    {
        services.AddSingleton<ITokenStore, InMemoryTokenStore>();
        services.AddSingleton(_ =>
        {
            var options = new AuthenticationClientOptions();
            configureAuthentication?.Invoke(options);
            return options;
        });
        services.AddTransient<BearerTokenHandler>();
        services.AddHttpClient<IAuthenticationApi, AuthenticationApi>(client =>
        {
            client.BaseAddress = new Uri(baseAddress);
            configureClient?.Invoke(client);
        });

        return services.AddHttpClient<IApiConnection, ApiConnection>(client =>
        {
            client.BaseAddress = new Uri(baseAddress);
            configureClient?.Invoke(client);
        }).AddHttpMessageHandler<BearerTokenHandler>();
    }

    public static IServiceCollection AddApiResourceClient<TInterface, TImplementation>(this IServiceCollection services)
        where TInterface : class
        where TImplementation : class, TInterface
        => services.AddTransient<TInterface, TImplementation>();
}
