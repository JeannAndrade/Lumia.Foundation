using Microsoft.Extensions.DependencyInjection;
using LumiaFoundation.Http.Client.Authentication;
using LumiaFoundation.Http.Client.Services;

namespace LumiaFoundation.Http.Client.Extensions;

public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Registra <see cref="IApiConnection"/>, <see cref="IAuthenticationApi"/> e o pipeline de
    /// autenticação (<see cref="BearerTokenHandler"/>) para o endereço base informado.
    /// </summary>
    /// <remarks>
    /// Este método NÃO registra <see cref="ITokenStore"/>. O consumidor deve registrar sua
    /// própria implementação (por exemplo, <see cref="InMemoryTokenStore"/> para processos
    /// únicos, ou uma implementação por sessão/usuário em aplicações web) antes de resolver
    /// <see cref="IApiConnection"/> ou <see cref="IAuthenticationApi"/>. Sem esse registro, a
    /// resolução de dependências falha ao construir <see cref="BearerTokenHandler"/>.
    /// </remarks>
    public static IHttpClientBuilder AddLumiaApiClient(
        this IServiceCollection services,
        string baseAddress,
        Action<HttpClient>? configureClient = null,
        Action<AuthenticationClientOptions>? configureAuthentication = null)
    {
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
