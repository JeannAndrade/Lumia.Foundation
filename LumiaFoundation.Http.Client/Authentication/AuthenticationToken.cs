namespace LumiaFoundation.Http.Client.Authentication;

/// <summary>Par de tokens retornado pelos endpoints de autenticação da API.</summary>
public sealed record AuthenticationToken(string AccessToken, string RefreshToken);
