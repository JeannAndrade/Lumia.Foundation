namespace LumiaFoundation.AspNetCore.ClientAuthentication;

public sealed class SessionTokenStoreOptions
{
    /// <summary>
    /// Chave usada para guardar o token na sessão. Use uma chave distinta por registro quando
    /// o host falar com mais de uma API (por exemplo, "Vault.ApiToken" e "TesouroDireto.ApiToken").
    /// </summary>
    public string SessionKey { get; set; } = "LumiaFoundation.ApiToken";
}
