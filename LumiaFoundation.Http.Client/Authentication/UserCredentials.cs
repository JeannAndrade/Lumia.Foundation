namespace LumiaFoundation.Http.Client.Authentication;

/// <summary>Credenciais aceitas pelo endpoint de login do Lumia.Foundation.Auth.</summary>
public sealed record UserCredentials(string UserName, string Password);
