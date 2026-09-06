using LumiaFoundation.EFRepository.Utils;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace LumiaFoundation.EFRepository.Extensions
{
    public static class ServiceExtensions
    {
        public static void ConfigureMariaDbDatabase<T>(this IServiceCollection services, DbConnectionHelper dbConfig, string migrationsAssembly) where T : DbContext
        {
            var serverVersion = new MariaDbServerVersion(new Version(dbConfig.MajorVersion, dbConfig.MinorVersion, dbConfig.BuildVersion));

            services.AddDbContext<T>(options =>
                options
                .UseMySql(dbConfig.GetConectionString(), serverVersion, b => b.MigrationsAssembly(migrationsAssembly))
                .LogTo(Console.WriteLine, LogLevel.Information)
                .EnableSensitiveDataLogging()
                .EnableDetailedErrors());
        }

        public static void ConfigureMySqlDbDatabase<T>(this IServiceCollection services, DbConnectionHelper dbConfig, string migrationsAssembly) where T : DbContext
        {
            services.AddDbContext<T>(options =>
                options
                .UseMySQL(dbConfig.GetConectionString(), b => b.MigrationsAssembly(migrationsAssembly))
                .LogTo(Console.WriteLine, LogLevel.Information)
                .EnableSensitiveDataLogging()
                .EnableDetailedErrors());
        }

        public static void ConfigurePostgreSqlDatabase<T>(this IServiceCollection services, DbConnectionHelper dbConfig, string migrationsAssembly) where T : DbContext
        {
            services.AddDbContext<T>(options =>
                options
                .UseNpgsql(dbConfig.GetConectionString(), b => b.MigrationsAssembly(migrationsAssembly))
                .LogTo(Console.WriteLine, LogLevel.Information)
                .EnableSensitiveDataLogging()
                .EnableDetailedErrors());
        }
    }
}
