namespace LumiaFoundation.AspNetCore.ClientAuthentication;

public sealed class ApiUnauthorizedExceptionHandlerOptions
{
    /// <summary>Para onde redirecionar quando a sessão da API expira e não pôde ser renovada.</summary>
    public string RedirectPath { get; set; } = "/Login?expired=true";
}
