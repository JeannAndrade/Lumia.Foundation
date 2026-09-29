using Microsoft.AspNetCore.Http;

namespace LumiaFoundation.AspNetCore.ClientAuthentication;

public interface IApiSignInService
{
    /// <summary>
    /// Autentica na API, guarda o par de tokens no <see cref="LumiaFoundation.Http.Client.Authentication.ITokenStore"/>
    /// e emite o sign-in local do host (por exemplo, o cookie de autenticação do site).
    /// </summary>
    Task<SignInOutcome> SignInAsync(
        HttpContext httpContext,
        string userName,
        string password,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Encerra o sign-in local do host e remove o par de tokens do <c>ITokenStore</c>.
    /// </summary>
    Task SignOutAsync(HttpContext httpContext, CancellationToken cancellationToken = default);
}
