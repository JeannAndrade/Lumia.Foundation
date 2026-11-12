
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Scalar.AspNetCore;

namespace LumiaFoundation.AspNetCore.Commons.Extensions;

public static class OpenApiEndpointRouteBuilderExtensions
{
    public static IEndpointRouteBuilder MapOpenApiDocuments(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapOpenApi();
        return endpoints;
    }

    public static IEndpointRouteBuilder MapScalarUi(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapScalarApiReference();
        return endpoints;
    }

    public static IEndpointRouteBuilder MapOpenApiDevTools(
        this IEndpointRouteBuilder endpoints,
        Func<IServiceProvider, bool>? shouldExpose = null)
    {
        var expose = shouldExpose?.Invoke(endpoints.ServiceProvider)
            ?? endpoints.ServiceProvider
                .GetRequiredService<IWebHostEnvironment>()
                .IsDevelopment();

        if (expose)
        {
            endpoints.MapOpenApiDocuments();
            endpoints.MapScalarUi();
        }

        return endpoints;
    }
}