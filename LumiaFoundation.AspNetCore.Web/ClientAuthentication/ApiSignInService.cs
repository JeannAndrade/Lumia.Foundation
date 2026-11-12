using System.Net;
using System.Security.Claims;
using LumiaFoundation.Http.Client.Authentication;
using LumiaFoundation.Http.Client.Exceptions;
using LumiaFoundation.Logger.Contracts;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using AuthenticationToken = LumiaFoundation.Http.Client.Authentication.AuthenticationToken;

namespace LumiaFoundation.AspNetCore.ClientAuthentication;

public sealed class ApiSignInService(
    IAuthenticationApi authenticationApi,
    ITokenStore tokenStore,
    ILoggerManager logger) : IApiSignInService
{
    private const string AuthenticationType = "LumiaApiSignIn";

    public async Task<SignInOutcome> SignInAsync(
        HttpContext httpContext,
        string userName,
        string password,
        CancellationToken cancellationToken = default)
    {
        AuthenticationToken token;

        try
        {
            token = await authenticationApi.LoginAsync(new UserCredentials(userName, password), cancellationToken);
        }
        catch (ApiException exception) when (exception.StatusCode == HttpStatusCode.Unauthorized)
        {
            logger.LogWarn("Credenciais recusadas pela API para o usuário {0}.", userName);
            return SignInOutcome.InvalidCredentials;
        }
        catch (ApiException exception)
        {
            logger.LogError(exception, "A API respondeu {0} ao autenticar o usuário.", (int)exception.StatusCode);
            return SignInOutcome.Unavailable;
        }
        catch (HttpRequestException exception)
        {
            logger.LogError(exception, "Não foi possível comunicar com a API de autenticação.");
            return SignInOutcome.Unavailable;
        }
        catch (TaskCanceledException exception) when (!cancellationToken.IsCancellationRequested)
        {
            // Timeout do HttpClient (o chamador não cancelou a requisição).
            logger.LogError(exception, "A API de autenticação não respondeu a tempo.");
            return SignInOutcome.Unavailable;
        }

        await tokenStore.SetAsync(token, cancellationToken);

        var identity = new ClaimsIdentity(
            new[] { new Claim(ClaimTypes.Name, userName) },
            AuthenticationType);

        // Sem especificar o esquema: usa o esquema padrão configurado pelo host
        // (AddAuthentication(defaultScheme)), qualquer que ele seja. Mantém esta classe
        // desacoplada de cookie/JWT/qualquer mecanismo específico de sign-in local.
        await httpContext.SignInAsync(new ClaimsPrincipal(identity));

        return SignInOutcome.Succeeded;
    }

    public async Task SignOutAsync(HttpContext httpContext, CancellationToken cancellationToken = default)
    {
        await httpContext.SignOutAsync();
        await tokenStore.ClearAsync(cancellationToken);
    }
}
