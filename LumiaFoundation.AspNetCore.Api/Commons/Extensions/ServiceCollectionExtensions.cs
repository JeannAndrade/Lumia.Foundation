using System.Reflection;
using System.Runtime.CompilerServices;
using LumiaFoundation.AspNetCore.Api.ActionFilters;
using LumiaFoundation.AspNetCore.Api.ExceptionHandlers;
using Microsoft.Extensions.DependencyInjection;

namespace LumiaFoundation.AspNetCore.Api.Commons.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddServicesFromAssembly(
        this IServiceCollection services,
        Assembly assembly,
        ServiceLifetime lifetime = ServiceLifetime.Scoped)
    {
        var types = assembly.GetTypes()
            .Where(t => t.IsClass
                && !t.IsAbstract
                && !t.IsNested
                && !t.IsDefined(typeof(CompilerGeneratedAttribute), inherit: false))
            .ToList();

        foreach (var implementationType in types)
        {
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

    public static void AddExceptionHandlers(this IServiceCollection services)
    {
        services.AddExceptionHandler<DomainExceptionHandler>();
        services.AddExceptionHandler<UnhandledExceptionHandler>();
    }
}
