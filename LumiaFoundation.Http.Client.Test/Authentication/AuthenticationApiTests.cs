using System.Net;
using System.Net.Http.Json;
using LumiaFoundation.Http.Client.Authentication;
using LumiaFoundation.Http.Client.Exceptions;
using LumiaFoundation.Http.Client.Test.TestDoubles;

namespace LumiaFoundation.Http.Client.Test.Authentication;

public class AuthenticationApiTests
{
    [Fact]
    public async Task LoginAndRefreshAsync_UseConfiguredPathsAndDeserializeTokens()
    {
        var handler = new StubHttpMessageHandler((request, _) => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = JsonContent.Create(new AuthenticationToken(
                request.RequestUri!.AbsolutePath == "/login" ? "access-login" : "access-refresh",
                "refresh"))
        });
        var api = new AuthenticationApi(
            new HttpClient(handler) { BaseAddress = new Uri("https://api.lumia.test/") },
            new AuthenticationClientOptions { LoginPath = "/login", RefreshPath = "/refresh" });

        var login = await api.LoginAsync(new UserCredentials("ana", "password"));
        var refresh = await api.RefreshAsync(login);

        Assert.Equal("access-login", login.AccessToken);
        Assert.Equal("access-refresh", refresh.AccessToken);
        Assert.Contains("access-login", handler.LastRequestContent);
    }

    [Fact]
    public async Task LoginAsync_WhenApiReturnsError_ThrowsApiException()
    {
        var handler = new StubHttpMessageHandler((_, _) => new HttpResponseMessage(HttpStatusCode.Unauthorized));
        var api = new AuthenticationApi(
            new HttpClient(handler) { BaseAddress = new Uri("https://api.lumia.test/") },
            new AuthenticationClientOptions());

        var exception = await Assert.ThrowsAsync<ApiException>(
            () => api.LoginAsync(new UserCredentials("ana", "invalid")));

        Assert.Equal(HttpStatusCode.Unauthorized, exception.StatusCode);
    }

    [Fact]
    public async Task LoginAsync_WhenSuccessHasNoTokenBody_ThrowsApiException()
    {
        var handler = new StubHttpMessageHandler((_, _) => new HttpResponseMessage(HttpStatusCode.OK));
        var api = new AuthenticationApi(
            new HttpClient(handler) { BaseAddress = new Uri("https://api.lumia.test/") },
            new AuthenticationClientOptions());

        var exception = await Assert.ThrowsAsync<ApiException>(
            () => api.LoginAsync(new UserCredentials("ana", "password")));

        Assert.Contains("sem tokens", exception.Message);
    }

    [Fact]
    public async Task LoginAsync_WhenSuccessHasInvalidJsonBody_ThrowsApiException()
    {
        var handler = new StubHttpMessageHandler((_, _) => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent("invalid json", System.Text.Encoding.UTF8, "application/json")
        });
        var api = new AuthenticationApi(
            new HttpClient(handler) { BaseAddress = new Uri("https://api.lumia.test/") },
            new AuthenticationClientOptions());

        var exception = await Assert.ThrowsAsync<ApiException>(
            () => api.LoginAsync(new UserCredentials("ana", "password")));

        Assert.Contains("sem tokens", exception.Message);
    }
}
