using LumiaFoundation.Logger.Contracts;
using LumiaFoundation.Logger.Extensions;
using LumiaFoundation.Logger.LoggerService;
using Microsoft.Extensions.DependencyInjection;

namespace LumiaFoundation.Logger.Test.Extensions;

public class ServiceExtensionsTests
{
    [Fact]
    public void ConfigureLoggerService_RegistersLoggerManagerAsSingleton()
    {
        // Arrange
        IServiceCollection services = new ServiceCollection();

        // Act
        services.ConfigureLoggerService();

        // Assert
        var descriptor = Assert.Single(services, item => item.ServiceType == typeof(ILoggerManager));
        Assert.Equal(ServiceLifetime.Singleton, descriptor.Lifetime);
        Assert.Equal(typeof(LoggerManager), descriptor.ImplementationType);
    }
}
