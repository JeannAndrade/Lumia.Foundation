namespace LumiaFoundation.AspNetCore.HealthChecks;

/// <summary>Rotas dos endpoints de health check mapeados por <c>MapLumiaHealthChecks</c>.</summary>
public static class LumiaHealthCheckPaths
{
    /// <summary>Liveness: o processo está de pé e respondendo. Não executa nenhuma check.</summary>
    public const string Live = "/health/live";

    /// <summary>Readiness: a aplicação está apta a receber requisições (checks com a tag Ready).</summary>
    public const string Ready = "/health/ready";
}
