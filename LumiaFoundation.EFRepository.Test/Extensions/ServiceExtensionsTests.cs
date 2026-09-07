using LumiaFoundation.EFRepository.Extensions;
using LumiaFoundation.EFRepository.Repository;
using LumiaFoundation.EFRepository.Utils;
using LumiaFoundation.EFRepository.Test.Repository;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace LumiaFoundation.EFRepository.Test.Extensions;

public class ServiceExtensionsTests
{
    [Fact]
    public void ConfigurePostgreSqlDatabase_RegistersTheDbContext()
    {
        // Arrange
        IServiceCollection services = new ServiceCollection();
        var helper = new DbConnectionHelper("localhost", "5432", "user", "password", "database", 10, 6, 12);

        // Act
        services.ConfigurePostgreSqlDatabase<TestRepositoryContext>(helper, "Migrations");

        // Assert
        Assert.Contains(services, descriptor => descriptor.ServiceType == typeof(TestRepositoryContext));
    }
}
