using LumiaFoundation.AspNetCore.ActionFilters;
using Microsoft.Extensions.DependencyInjection;

namespace LumiaFoundation.AspNetCore.Extensions
{
    public static class ServiceExtensions
    {

        public static void AddValidationFilters(this IServiceCollection services)
        {
            services.AddScoped<DtoNotEmptyValidationAttribute>();
        }

    }
}