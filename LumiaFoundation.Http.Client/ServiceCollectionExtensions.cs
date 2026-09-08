using Microsoft.Extensions.DependencyInjection;

namespace LumiaFoundation.Http.Client;

public static class ServiceCollectionExtensions
{
    public static IHttpClientBuilder AddLumiaApiClient(
        this IServiceCollection services,
        string baseAddress,
        Action<HttpClient>? configureClient = null)
    {
        return services.AddHttpClient<IApiConnection, ApiConnection>(client =>
        {
            client.BaseAddress = new Uri(baseAddress);
            configureClient?.Invoke(client);
        });
    }

    public static IServiceCollection AddApiResourceClient<TInterface, TImplementation>(this IServiceCollection services)
        where TInterface : class
        where TImplementation : class, TInterface
        => services.AddTransient<TInterface, TImplementation>();
}