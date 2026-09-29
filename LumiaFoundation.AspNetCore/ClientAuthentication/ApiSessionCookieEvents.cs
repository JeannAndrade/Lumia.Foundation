using LumiaFoundation.Http.Client.Authentication;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;

namespace LumiaFoundation.AspNetCore.ClientAuthentication;

/// <summary>
/// Rejeita o cookie de autenticação quando não há mais um token de API associado à sessão
/// atual — por exemplo, após o processo reiniciar e o cache de sessão em memória ser perdido.
/// </summary>
/// <remarks>
/// Registrado via <c>options.EventsType = typeof(ApiSessionCookieEvents)</c>, para que o
/// ASP.NET Core resolva esta classe (e o <see cref="ITokenStore"/> que ela depende) a partir
/// do contêiner de DI, por requisição.
/// </remarks>
public sealed class ApiSessionCookieEvents(ITokenStore tokenStore) : CookieAuthenticationEvents
{
    public override async Task ValidatePrincipal(CookieValidatePrincipalContext context)
    {
        var token = await tokenStore.GetAsync(context.HttpContext.RequestAborted);

        if (token is null)
        {
            context.RejectPrincipal();
            await context.HttpContext.SignOutAsync(context.Scheme.Name);
        }
    }
}
