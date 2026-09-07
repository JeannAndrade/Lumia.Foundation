using LumiaFoundation.AspNetCore.ServiceFilters;
using Microsoft.Extensions.DependencyInjection;

namespace LumiaFoundation.AspNetCore.Commons.Extensions;

public static class DomainExceptionMappingFilterExtensions
{
  public static IServiceCollection AddDomainExceptionMappingFilter(this IServiceCollection services)
  {
    services.AddScoped<DomainExceptionMappingFilter>();

    return services;
  }
}