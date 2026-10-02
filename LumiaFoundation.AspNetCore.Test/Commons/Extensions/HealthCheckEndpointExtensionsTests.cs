using System.Net;
using LumiaFoundation.AspNetCore.Commons.Extensions;
using LumiaFoundation.AspNetCore.HealthChecks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Hosting;

namespace LumiaFoundation.AspNetCore.Test.Commons.Extensions;

public class HealthCheckEndpointExtensionsTests
{
    [Fact]
    public async Task Live_WhenReadyCheckIsUnhealthy_StillReturnsOk()
    {
        // Arrange: o liveness não pode depender das checks de readiness
        using var host = await StartHostAsync(checks => checks.AddCheck(
            "banco", () => HealthCheckResult.Unhealthy("falha"), tags: [LumiaHealthCheckTags.Ready]));
        using var client = host.GetTestClient();

        // Act
        var response = await client.GetAsync(LumiaHealthCheckPaths.Live);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("Healthy", await response.Content.ReadAsStringAsync());
    }

    [Fact]
    public async Task Ready_WhenReadyChecksAreHealthy_ReturnsOk()
    {
        // Arrange
        using var host = await StartHostAsync(checks => checks.AddCheck(
            "banco", () => HealthCheckResult.Healthy(), tags: [LumiaHealthCheckTags.Ready]));
        using var client = host.GetTestClient();

        // Act
        var response = await client.GetAsync(LumiaHealthCheckPaths.Ready);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("Healthy", await response.Content.ReadAsStringAsync());
    }

    [Fact]
    public async Task Ready_WhenReadyCheckIsUnhealthy_ReturnsServiceUnavailableWithoutDetails()
    {
        // Arrange
        using var host = await StartHostAsync(checks => checks.AddCheck(
            "banco", () => HealthCheckResult.Unhealthy("detalhe interno"), tags: [LumiaHealthCheckTags.Ready]));
        using var client = host.GetTestClient();

        // Act
        var response = await client.GetAsync(LumiaHealthCheckPaths.Ready);
        var body = await response.Content.ReadAsStringAsync();

        // Assert
        Assert.Equal(HttpStatusCode.ServiceUnavailable, response.StatusCode);
        Assert.Equal("Unhealthy", body);
        Assert.DoesNotContain("detalhe interno", body);
        Assert.DoesNotContain("banco", body);
    }

    [Fact]
    public async Task Ready_WhenUnhealthyCheckHasNoReadyTag_IgnoresIt()
    {
        // Arrange
        using var host = await StartHostAsync(checks => checks.AddCheck(
            "outra", () => HealthCheckResult.Unhealthy("falha")));
        using var client = host.GetTestClient();

        // Act
        var response = await client.GetAsync(LumiaHealthCheckPaths.Ready);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Theory]
    [InlineData(LumiaHealthCheckPaths.Live)]
    [InlineData(LumiaHealthCheckPaths.Ready)]
    public async Task Endpoints_WhenAuthorizationRequiresAuthenticatedUser_AreStillAnonymous(string path)
    {
        // Arrange: com política global exigindo usuário autenticado, a sonda não pode ser bloqueada.
        // Sem AllowAnonymous, o fallback tentaria um challenge e a requisição falharia.
        using var host = await StartHostAsync(
            checks => checks.AddCheck("banco", () => HealthCheckResult.Healthy(), tags: [LumiaHealthCheckTags.Ready]),
            requireAuthenticatedUser: true);
        using var client = host.GetTestClient();

        // Act
        var response = await client.GetAsync(path);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public void MapLumiaHealthChecks_WhenEndpointsIsNull_ThrowsArgumentNullException()
    {
        // Arrange
        Microsoft.AspNetCore.Routing.IEndpointRouteBuilder endpoints = null!;

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => endpoints.MapLumiaHealthChecks());
    }

    [Fact]
    public void Constants_ExposeTheContractUsedByOrchestrators()
    {
        // Assert: estes valores são usados pelas sondas do Docker; mudá-los é uma quebra de contrato.
        Assert.Equal("/health/live", LumiaHealthCheckPaths.Live);
        Assert.Equal("/health/ready", LumiaHealthCheckPaths.Ready);
        Assert.Equal("ready", LumiaHealthCheckTags.Ready);
    }

    private static Task<IHost> StartHostAsync(
        Action<IHealthChecksBuilder> configureChecks,
        bool requireAuthenticatedUser = false) =>
        new HostBuilder()
            .ConfigureWebHost(web => web
                .UseTestServer()
                .ConfigureServices(services =>
                {
                    services.AddRouting();
                    services.AddAuthorization(options =>
                    {
                        if (requireAuthenticatedUser)
                            options.FallbackPolicy = new AuthorizationPolicyBuilder().RequireAuthenticatedUser().Build();
                    });
                    configureChecks(services.AddHealthChecks());
                })
                .Configure(app =>
                {
                    app.UseRouting();
                    app.UseAuthorization();
                    app.UseEndpoints(endpoints => endpoints.MapLumiaHealthChecks());
                }))
            .StartAsync();
}
