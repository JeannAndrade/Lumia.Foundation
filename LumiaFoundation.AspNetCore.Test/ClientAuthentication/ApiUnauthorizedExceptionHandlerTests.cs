using System.Net;
using LumiaFoundation.AspNetCore.ClientAuthentication;
using LumiaFoundation.AspNetCore.Test.TestDoubles;
using LumiaFoundation.Http.Client.Exceptions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Moq;

namespace LumiaFoundation.AspNetCore.Test.ClientAuthentication;

public class ApiUnauthorizedExceptionHandlerTests
{
    private readonly Mock<IApiSignInService> _signInService = new();
    private readonly DefaultHttpContext _httpContext;
    private readonly ApiUnauthorizedExceptionHandler _handler;

    public ApiUnauthorizedExceptionHandlerTests()
    {
        _handler = new ApiUnauthorizedExceptionHandler(
            new FakeLoggerManager(),
            Options.Create(new ApiUnauthorizedExceptionHandlerOptions()));

        _httpContext = new DefaultHttpContext
        {
            RequestServices = new ServiceCollection()
                .AddSingleton(_signInService.Object)
                .BuildServiceProvider()
        };
    }

    [Fact]
    public async Task TryHandleAsync_WhenExceptionIsApiUnauthorized_SignsOutAndRedirectsToLogin()
    {
        var handled = await _handler.TryHandleAsync(
            _httpContext, new ApiException(HttpStatusCode.Unauthorized, "expirou"), CancellationToken.None);

        Assert.True(handled);
        _signInService.Verify(s => s.SignOutAsync(_httpContext, It.IsAny<CancellationToken>()), Times.Once);
        Assert.Equal("/Login?expired=true", _httpContext.Response.Headers.Location.ToString());
    }

    [Fact]
    public async Task TryHandleAsync_WhenApiExceptionIsNotUnauthorized_ReturnsFalse()
    {
        var handled = await _handler.TryHandleAsync(
            _httpContext, new ApiException(HttpStatusCode.InternalServerError, "erro"), CancellationToken.None);

        Assert.False(handled);
        _signInService.Verify(s => s.SignOutAsync(It.IsAny<HttpContext>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task TryHandleAsync_WhenExceptionIsNotApiException_ReturnsFalse()
    {
        var handled = await _handler.TryHandleAsync(
            _httpContext, new InvalidOperationException("boom"), CancellationToken.None);

        Assert.False(handled);
    }

    [Fact]
    public async Task TryHandleAsync_UsesConfiguredRedirectPath()
    {
        var handler = new ApiUnauthorizedExceptionHandler(
            new FakeLoggerManager(),
            Options.Create(new ApiUnauthorizedExceptionHandlerOptions { RedirectPath = "/Entrar" }));

        await handler.TryHandleAsync(
            _httpContext, new ApiException(HttpStatusCode.Unauthorized, "expirou"), CancellationToken.None);

        Assert.Equal("/Entrar", _httpContext.Response.Headers.Location.ToString());
    }
}
