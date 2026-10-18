using System.Reflection;
using System.Runtime.CompilerServices;
using LumiaFoundation.AspNetCore.ActionFilters;
using LumiaFoundation.AspNetCore.ClientAuthentication;
using LumiaFoundation.Http.Client.Authentication;
using Microsoft.Extensions.DependencyInjection;

namespace LumiaFoundation.AspNetCore.Commons.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddServicesFromAssembly(
    this IServiceCollection services,
    Assembly assembly,
    ServiceLifetime lifetime = ServiceLifetime.Scoped)
    {
        // Classes concretas escritas pelo desenvolvedor: ignora abstratas, interfaces, tipos aninhados
        // e tipos sintetizados pelo compilador (ex.: <>z__ReadOnlySingleElementList<T> e seu Enumerator).
        var types = assembly.GetTypes()
            .Where(t => t.IsClass
                     && !t.IsAbstract
                     && !t.IsNested
                     && !t.IsDefined(typeof(CompilerGeneratedAttribute), inherit: false))
            .ToList();

        foreach (var implementationType in types)
        {
            // Convenção "I" + NomeDaClasse, e a interface precisa ser do MESMO assembly:
            // evita casar por acidente com System.Collections.IEnumerator, IDisposable etc.
            var interfaceType = implementationType.GetInterfaces()
                .FirstOrDefault(i => i.Assembly == assembly && i.Name == $"I{implementationType.Name}");

            if (interfaceType != null)
            {
                services.Add(new ServiceDescriptor(interfaceType, implementationType, lifetime));
            }
        }

        return services;
    }

    public static void AddValidationFilters(this IServiceCollection services)
    {
        services.AddScoped<DtoNotEmptyValidationAttribute>();
    }

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
