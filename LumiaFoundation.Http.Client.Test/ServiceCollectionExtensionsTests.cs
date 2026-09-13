using Microsoft.Extensions.DependencyInjection;
using LumiaFoundation.Http.Client.Authentication;

namespace LumiaFoundation.Http.Client.Test;

public class ServiceCollectionExtensionsTests
{
    [Fact]
    public void AddLumiaApiClient_RegistersApiConnectionWithConfiguredClient()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        var builder = services.AddLumiaApiClient("https://api.lumia.test/v1/", client =>
            client.DefaultRequestHeaders.Add("X-Client", "tests"));
        using var provider = services.BuildServiceProvider();
        var factory = provider.GetRequiredService<IHttpClientFactory>();
        var client = factory.CreateClient(nameof(IApiConnection));
        var connection = provider.GetRequiredService<IApiConnection>();

        // Assert
        Assert.NotNull(builder);
        Assert.Equal(new Uri("https://api.lumia.test/v1/"), client.BaseAddress);
        Assert.Contains("tests", client.DefaultRequestHeaders.GetValues("X-Client"));
        Assert.IsType<ApiConnection>(connection);
    }

    [Fact]
    public void AddLumiaApiClient_WithoutConfigureActions_RegistersServices()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        var builder = services.AddLumiaApiClient("https://api.lumia.test/v1/");
        using var provider = services.BuildServiceProvider();
        var client = provider.GetRequiredService<IApiConnection>();
        var authApi = provider.GetRequiredService<IAuthenticationApi>();

        // Assert
        Assert.NotNull(builder);
        Assert.IsType<ApiConnection>(client);
        Assert.IsType<AuthenticationApi>(authApi);
    }

    [Fact]
    public void AddApiResourceClient_RegistersTransientImplementation()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        var returnedServices = services.AddApiResourceClient<IWeatherResource, WeatherResource>();
        using var provider = services.BuildServiceProvider();
        var first = provider.GetRequiredService<IWeatherResource>();
        var second = provider.GetRequiredService<IWeatherResource>();

        // Assert
        Assert.Same(services, returnedServices);
        Assert.IsType<WeatherResource>(first);
        Assert.NotSame(first, second);
    }

    private interface IWeatherResource;
    private sealed class WeatherResource : IWeatherResource;
}

