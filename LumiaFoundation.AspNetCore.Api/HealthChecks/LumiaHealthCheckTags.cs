namespace LumiaFoundation.AspNetCore.HealthChecks;

/// <summary>Tags usadas pelos endpoints de health check da Lumia Foundation.</summary>
public static class LumiaHealthCheckTags
{
    /// <summary>Checks com esta tag participam do endpoint de readiness.</summary>
    public const string Ready = "ready";
}
