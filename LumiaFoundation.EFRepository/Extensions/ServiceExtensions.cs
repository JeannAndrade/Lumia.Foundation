using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace LumiaFoundation.EFRepository.Extensions
{
    public static class ServiceExtensions
    {
        public static void ConfigureMariaDbDatabase<T>(this IServiceCollection services, MariaDbConfiguration dbConfig) where T : DbContext
        {
            var serverVersion = new MariaDbServerVersion(new Version(dbConfig.MajorVersion, dbConfig.MinorVersion, dbConfig.BuildVersion));

            services.AddDbContext<T>(options =>
                options
                .UseMySql(dbConfig.GetConectionString(), serverVersion)
                .LogTo(Console.WriteLine, LogLevel.Information)
                .EnableSensitiveDataLogging()
                .EnableDetailedErrors());
        }
    }
}