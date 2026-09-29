using System.Net;
using LumiaFoundation.Http.Client.Authentication;
using LumiaFoundation.Http.Client.Exceptions;
using LumiaFoundation.Logger.Contracts;

namespace LumiaFoundation.AspNetCore.ClientAuthentication;

public sealed class ApiRegistrationService(
    IAuthenticationApi authenticationApi,
    ILoggerManager logger) : IApiRegistrationService
{
    public async Task<RegistrationResult> RegisterAsync(
        UserRegistration registration, CancellationToken cancellationToken = default)
    {
        try
        {
            await authenticationApi.RegisterAsync(registration, cancellationToken);
            return RegistrationResult.Succeeded();
        }
        catch (ApiException exception) when (exception.StatusCode == HttpStatusCode.UnprocessableEntity)
        {
            logger.LogWarn("Registro recusado pela API para o usuário {0}: {1}", registration.UserName, exception.Message);
            return RegistrationResult.Rejected(exception.Message);
        }
        catch (ApiException exception)
        {
            logger.LogError(exception, "A API respondeu {0} ao registrar o usuário.", (int)exception.StatusCode);
            return RegistrationResult.Unavailable();
        }
        catch (HttpRequestException exception)
        {
            logger.LogError(exception, "Não foi possível comunicar com a API de autenticação.");
            return RegistrationResult.Unavailable();
        }
        catch (TaskCanceledException exception) when (!cancellationToken.IsCancellationRequested)
        {
            logger.LogError(exception, "A API de autenticação não respondeu a tempo.");
            return RegistrationResult.Unavailable();
        }
    }
}
