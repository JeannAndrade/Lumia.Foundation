using LumiaFoundation.EFRepository.Utils;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace LumiaFoundation.EFRepository.Extensions;

public static class ServiceExtensions
{
    private static bool IsDevelopmentEnvironment()
    {
        var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production";
        return environment.Equals("Development", StringComparison.OrdinalIgnoreCase);
    }

    private static void ApplyDevelopmentOptions(DbContextOptionsBuilder options)
    {
        options
            .LogTo(Console.WriteLine, LogLevel.Information)
            .EnableSensitiveDataLogging()
            .EnableDetailedErrors();
    }

    public static void ConfigureMariaDbDatabase<T>(this IServiceCollection services, DbConnectionHelper dbConfig, string migrationsAssembly) where T : DbContext
    {
        var serverVersion = new MariaDbServerVersion(new Version(dbConfig.MajorVersion, dbConfig.MinorVersion, dbConfig.BuildVersion));
        var isDevelopment = IsDevelopmentEnvironment();

        services.AddDbContext<T>(options =>
        {
            options
                .UseMySql(dbConfig.GetConectionString(), serverVersion, b => b.MigrationsAssembly(migrationsAssembly));

            if (isDevelopment)
            {
                ApplyDevelopmentOptions(options);
            }
        });
    }

    public static void ConfigureMySqlDbDatabase<T>(this IServiceCollection services, DbConnectionHelper dbConfig, string migrationsAssembly) where T : DbContext
    {
        var isDevelopment = IsDevelopmentEnvironment();

        services.AddDbContext<T>(options =>
        {
            options
                .UseMySQL(dbConfig.GetConectionString(), b => b.MigrationsAssembly(migrationsAssembly));

            if (isDevelopment)
            {
                ApplyDevelopmentOptions(options);
            }
        });
    }

    public static void ConfigurePostgreSqlDatabase<T>(this IServiceCollection services, DbConnectionHelper dbConfig, string migrationsAssembly) where T : DbContext
    {
        var isDevelopment = IsDevelopmentEnvironment();

        services.AddDbContext<T>(options =>
        {
            options
                .UseNpgsql(dbConfig.GetConectionString(), b => b.MigrationsAssembly(migrationsAssembly));

            if (isDevelopment)
            {
                ApplyDevelopmentOptions(options);
            }
        });
    }
}
