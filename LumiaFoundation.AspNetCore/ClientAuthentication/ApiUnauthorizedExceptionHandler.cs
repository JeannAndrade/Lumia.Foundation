using System.Net;
using LumiaFoundation.Http.Client.Exceptions;
using LumiaFoundation.Logger.Contracts;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace LumiaFoundation.AspNetCore.ClientAuthentication;

/// <summary>
/// Quando a API responde 401 e a sessão não pôde ser renovada, encerra o sign-in local e
/// leva o usuário de volta ao login.
/// </summary>
public sealed class ApiUnauthorizedExceptionHandler(
    ILoggerManager logger,
    IOptions<ApiUnauthorizedExceptionHandlerOptions> options) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        if (exception is not ApiException { StatusCode: HttpStatusCode.Unauthorized })
            return false;

        logger.LogInfo("A API respondeu 401; encerrando a sessão do usuário e redirecionando para o login.");

        var signInService = httpContext.RequestServices.GetRequiredService<IApiSignInService>();
        await signInService.SignOutAsync(httpContext, cancellationToken);

        httpContext.Response.Redirect(options.Value.RedirectPath);

        return true;
    }
}
