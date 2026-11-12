using System.Net;
using System.Security.Claims;
using LumiaFoundation.AspNetCore.ClientAuthentication;
using LumiaFoundation.AspNetCore.Test.TestDoubles;
using LumiaFoundation.Http.Client.Authentication;
using LumiaFoundation.Http.Client.Exceptions;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using AuthenticationToken = LumiaFoundation.Http.Client.Authentication.AuthenticationToken;

namespace LumiaFoundation.AspNetCore.Test.ClientAuthentication;

public class ApiSignInServiceTests
{
    private readonly Mock<IAuthenticationApi> _authenticationApi = new();
    private readonly Mock<ITokenStore> _tokenStore = new();
    private readonly Mock<IAuthenticationService> _authenticationService = new();
    private readonly FakeLoggerManager _logger = new();
    private readonly DefaultHttpContext _httpContext;
    private readonly ApiSignInService _service;

    public ApiSignInServiceTests()
    {
        _service = new ApiSignInService(_authenticationApi.Object, _tokenStore.Object, _logger);
        _httpContext = new DefaultHttpContext
        {
            RequestServices = new ServiceCollection()
                .AddSingleton(_authenticationService.Object)
                .BuildServiceProvider()
        };
    }

    private void SetupLoginToReturn(AuthenticationToken token) =>
        _authenticationApi
            .Setup(a => a.LoginAsync(It.IsAny<UserCredentials>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(token);

    private void SetupLoginToThrow(Exception exception) =>
        _authenticationApi
            .Setup(a => a.LoginAsync(It.IsAny<UserCredentials>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(exception);

    private void VerifyNothingWasStoredNorSignedIn()
    {
        _tokenStore.Verify(
            t => t.SetAsync(It.IsAny<AuthenticationToken>(), It.IsAny<CancellationToken>()), Times.Never);
        _authenticationService.Verify(
            a => a.SignInAsync(It.IsAny<HttpContext>(), It.IsAny<string?>(), It.IsAny<ClaimsPrincipal>(), It.IsAny<AuthenticationProperties?>()),
            Times.Never);
    }

    [Fact]
    public async Task SignInAsync_WhenCredentialsAreValid_StoresTokenAndSignsInWithUserName()
    {
        var token = new AuthenticationToken("access", "refresh");
        SetupLoginToReturn(token);

        var outcome = await _service.SignInAsync(_httpContext, "ana", "senha");

        Assert.Equal(SignInOutcome.Succeeded, outcome);
        _tokenStore.Verify(t => t.SetAsync(token, It.IsAny<CancellationToken>()), Times.Once);
        _authenticationService.Verify(a => a.SignInAsync(
            _httpContext,
            null, // sem esquema explícito: usa o esquema padrão do host
            It.Is<ClaimsPrincipal>(p => p.Identity!.Name == "ana"),
            It.IsAny<AuthenticationProperties?>()), Times.Once);
    }

    [Fact]
    public async Task SignInAsync_WhenApiReturnsUnauthorized_ReturnsInvalidCredentials()
    {
        SetupLoginToThrow(new ApiException(HttpStatusCode.Unauthorized, "não autorizado"));

        var outcome = await _service.SignInAsync(_httpContext, "ana", "errada");

        Assert.Equal(SignInOutcome.InvalidCredentials, outcome);
        VerifyNothingWasStoredNorSignedIn();
        Assert.Single(_logger.WarnMessages);
    }

    [Fact]
    public async Task SignInAsync_WhenApiFails_ReturnsUnavailable()
    {
        SetupLoginToThrow(new ApiException(HttpStatusCode.InternalServerError, "erro"));

        var outcome = await _service.SignInAsync(_httpContext, "ana", "senha");

        Assert.Equal(SignInOutcome.Unavailable, outcome);
        VerifyNothingWasStoredNorSignedIn();
    }

    [Fact]
    public async Task SignInAsync_WhenApiIsUnreachable_ReturnsUnavailable()
    {
        SetupLoginToThrow(new HttpRequestException("sem conexão"));

        var outcome = await _service.SignInAsync(_httpContext, "ana", "senha");

        Assert.Equal(SignInOutcome.Unavailable, outcome);
        VerifyNothingWasStoredNorSignedIn();
    }

    [Fact]
    public async Task SignInAsync_WhenHttpClientTimesOut_ReturnsUnavailable()
    {
        SetupLoginToThrow(new TaskCanceledException("timeout", new TimeoutException()));

        var outcome = await _service.SignInAsync(_httpContext, "ana", "senha");

        Assert.Equal(SignInOutcome.Unavailable, outcome);
        VerifyNothingWasStoredNorSignedIn();
    }

    [Fact]
    public async Task SignInAsync_WhenRequestIsCancelledByCaller_PropagatesTheCancellation()
    {
        using var cts = new CancellationTokenSource();
        cts.Cancel();
        SetupLoginToThrow(new TaskCanceledException());

        await Assert.ThrowsAsync<TaskCanceledException>(
            () => _service.SignInAsync(_httpContext, "ana", "senha", cts.Token));
    }

    [Fact]
    public async Task SignOutAsync_SignsOutAndClearsTheToken()
    {
        await _service.SignOutAsync(_httpContext);

        _authenticationService.Verify(a => a.SignOutAsync(
            _httpContext, null, It.IsAny<AuthenticationProperties?>()), Times.Once);
        _tokenStore.Verify(t => t.ClearAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}
