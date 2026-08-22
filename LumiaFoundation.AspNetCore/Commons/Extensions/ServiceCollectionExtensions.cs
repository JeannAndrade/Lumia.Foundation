using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;

namespace LumiaFoundation.AspNetCore.Commons.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddServicesFromAssembly(
            this IServiceCollection services,
            Assembly assembly,
            ServiceLifetime lifetime = ServiceLifetime.Scoped)
        {
            // Pega todas as classes concretas (não abstratas, não interfaces)
            var types = assembly.GetTypes()
                .Where(t => t.IsClass && !t.IsAbstract && !t.IsInterface)
                .ToList();

            foreach (var implementationType in types)
            {
                // Busca a interface que segue a convenção "I" + NomeDaClasse
                var interfaceType = implementationType.GetInterfaces()
                    .FirstOrDefault(i => i.Name == $"I{implementationType.Name}");

                if (interfaceType != null)
                {
                    services.Add(new ServiceDescriptor(interfaceType, implementationType, lifetime));
                }
            }

            return services;
        }
    }
}