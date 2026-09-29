using System.Security.Claims;
using LumiaFoundation.AspNetCore.ClientAuthentication;
using LumiaFoundation.Http.Client.Authentication;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using AuthenticationToken = LumiaFoundation.Http.Client.Authentication.AuthenticationToken;

namespace LumiaFoundation.AspNetCore.Test.ClientAuthentication;

public class ApiSessionCookieEventsTests
{
    private static (DefaultHttpContext HttpContext, CookieValidatePrincipalContext Context) CreateContext(
        Mock<IAuthenticationService> authenticationService)
    {
        var httpContext = new DefaultHttpContext
        {
            RequestServices = new ServiceCollection()
                .AddSingleton(authenticationService.Object)
                .BuildServiceProvider()
        };

        var scheme = new AuthenticationScheme(
            CookieAuthenticationDefaults.AuthenticationScheme, null, typeof(CookieAuthenticationHandler));

        var identity = new ClaimsIdentity(
            new[] { new Claim(ClaimTypes.Name, "ana") }, CookieAuthenticationDefaults.AuthenticationScheme);
        var ticket = new AuthenticationTicket(
            new ClaimsPrincipal(identity), CookieAuthenticationDefaults.AuthenticationScheme);

        var context = new CookieValidatePrincipalContext(
            httpContext, scheme, new CookieAuthenticationOptions(), ticket);

        return (httpContext, context);
    }

    [Fact]
    public async Task ValidatePrincipal_WhenTokenStoreHasNoToken_RejectsThePrincipalAndSignsOut()
    {
        var tokenStore = new Mock<ITokenStore>();
        tokenStore.Setup(t => t.GetAsync(It.IsAny<CancellationToken>())).ReturnsAsync((AuthenticationToken?)null);
        var authenticationService = new Mock<IAuthenticationService>();
        var (httpContext, context) = CreateContext(authenticationService);
        var events = new ApiSessionCookieEvents(tokenStore.Object);

        await events.ValidatePrincipal(context);

        Assert.Null(context.Principal);
        authenticationService.Verify(a => a.SignOutAsync(
            httpContext, CookieAuthenticationDefaults.AuthenticationScheme, It.IsAny<AuthenticationProperties?>()),
            Times.Once);
    }

    [Fact]
    public async Task ValidatePrincipal_WhenTokenStoreHasToken_KeepsThePrincipal()
    {
        var tokenStore = new Mock<ITokenStore>();
        tokenStore.Setup(t => t.GetAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new AuthenticationToken("access", "refresh"));
        var authenticationService = new Mock<IAuthenticationService>();
        var (_, context) = CreateContext(authenticationService);
        var events = new ApiSessionCookieEvents(tokenStore.Object);

        await events.ValidatePrincipal(context);

        Assert.NotNull(context.Principal);
        authenticationService.Verify(a => a.SignOutAsync(
            It.IsAny<HttpContext>(), It.IsAny<string>(), It.IsAny<AuthenticationProperties?>()), Times.Never);
    }
}
