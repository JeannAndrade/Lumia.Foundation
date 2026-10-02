using LumiaFoundation.AspNetCore.HealthChecks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Routing;

namespace LumiaFoundation.AspNetCore.Commons.Extensions;

public static class HealthCheckEndpointExtensions
{
    /// <summary>
    /// Mapeia <see cref="LumiaHealthCheckPaths.Live"/> (sem executar checks) e
    /// <see cref="LumiaHealthCheckPaths.Ready"/> (apenas checks com a tag <see cref="LumiaHealthCheckTags.Ready"/>).
    /// Os endpoints são anônimos e respondem só o status em texto puro ("Healthy"/"Unhealthy"),
    /// sem detalhes das checks. Requer <c>services.AddHealthChecks()</c>.
    /// </summary>
    public static IEndpointRouteBuilder MapLumiaHealthChecks(this IEndpointRouteBuilder endpoints)
    {
        ArgumentNullException.ThrowIfNull(endpoints);

        endpoints
            .MapHealthChecks(LumiaHealthCheckPaths.Live, new HealthCheckOptions
            {
                // Nenhuma check é executada: responde 200 enquanto o processo atender requisições.
                Predicate = _ => false
            })
            .AllowAnonymous();

        endpoints
            .MapHealthChecks(LumiaHealthCheckPaths.Ready, new HealthCheckOptions
            {
                Predicate = registration => registration.Tags.Contains(LumiaHealthCheckTags.Ready)
            })
            .AllowAnonymous();

        return endpoints;
    }
}
