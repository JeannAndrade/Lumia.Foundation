namespace LumiaFoundation.Http.Client.Authentication;

/// <summary>Rotas da API de autenticação. Ajuste-as ao controller da aplicação consumida.</summary>
public sealed class AuthenticationClientOptions
{
    public string LoginPath { get; set; } = "/api/authentication";
    public string RefreshPath { get; set; } = "/api/authentication/refresh";
}
