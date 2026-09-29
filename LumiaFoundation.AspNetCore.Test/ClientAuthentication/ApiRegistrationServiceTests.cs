using System.Net;
using LumiaFoundation.AspNetCore.ClientAuthentication;
using LumiaFoundation.AspNetCore.Test.TestDoubles;
using LumiaFoundation.Http.Client.Authentication;
using LumiaFoundation.Http.Client.Exceptions;
using Moq;

namespace LumiaFoundation.AspNetCore.Test.ClientAuthentication;

public class ApiRegistrationServiceTests
{
    private static readonly UserRegistration SampleRegistration =
        new("Ana", "Silva", "ana", "Senha@12345", "ana@exemplo.com");

    private readonly Mock<IAuthenticationApi> _authenticationApi = new();
    private readonly ApiRegistrationService _service;

    public ApiRegistrationServiceTests()
    {
        _service = new ApiRegistrationService(_authenticationApi.Object, new FakeLoggerManager());
    }

    [Fact]
    public async Task RegisterAsync_WhenApiSucceeds_ReturnsSucceeded()
    {
        _authenticationApi
            .Setup(a => a.RegisterAsync(SampleRegistration, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var result = await _service.RegisterAsync(SampleRegistration);

        Assert.Equal(RegistrationOutcome.Succeeded, result.Outcome);
        Assert.Null(result.ErrorMessage);
    }

    [Fact]
    public async Task RegisterAsync_WhenApiReturnsUnprocessableEntity_ReturnsRejectedWithApiMessage()
    {
        _authenticationApi
            .Setup(a => a.RegisterAsync(It.IsAny<UserRegistration>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new ApiException(HttpStatusCode.UnprocessableEntity, "Username 'ana' is already taken."));

        var result = await _service.RegisterAsync(SampleRegistration);

        Assert.Equal(RegistrationOutcome.Rejected, result.Outcome);
        Assert.Equal("Username 'ana' is already taken.", result.ErrorMessage);
    }

    [Fact]
    public async Task RegisterAsync_WhenApiFailsWithOtherStatus_ReturnsUnavailable()
    {
        _authenticationApi
            .Setup(a => a.RegisterAsync(It.IsAny<UserRegistration>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new ApiException(HttpStatusCode.InternalServerError, "erro"));

        var result = await _service.RegisterAsync(SampleRegistration);

        Assert.Equal(RegistrationOutcome.Unavailable, result.Outcome);
    }

    [Fact]
    public async Task RegisterAsync_WhenApiIsUnreachable_ReturnsUnavailable()
    {
        _authenticationApi
            .Setup(a => a.RegisterAsync(It.IsAny<UserRegistration>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new HttpRequestException("sem conexão"));

        var result = await _service.RegisterAsync(SampleRegistration);

        Assert.Equal(RegistrationOutcome.Unavailable, result.Outcome);
    }

    [Fact]
    public async Task RegisterAsync_WhenHttpClientTimesOut_ReturnsUnavailable()
    {
        _authenticationApi
            .Setup(a => a.RegisterAsync(It.IsAny<UserRegistration>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new TaskCanceledException("timeout", new TimeoutException()));

        var result = await _service.RegisterAsync(SampleRegistration);

        Assert.Equal(RegistrationOutcome.Unavailable, result.Outcome);
    }

    [Fact]
    public async Task RegisterAsync_WhenRequestIsCancelledByCaller_PropagatesTheCancellation()
    {
        using var cts = new CancellationTokenSource();
        cts.Cancel();
        _authenticationApi
            .Setup(a => a.RegisterAsync(It.IsAny<UserRegistration>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new TaskCanceledException());

        await Assert.ThrowsAsync<TaskCanceledException>(
            () => _service.RegisterAsync(SampleRegistration, cts.Token));
    }
}
