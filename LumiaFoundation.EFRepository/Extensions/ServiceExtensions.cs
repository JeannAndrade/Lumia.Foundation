using LumiaFoundation.EFRepository.Identity.Model;
using LumiaFoundation.EFRepository.Repository.IdentityBase;
using Microsoft.AspNetCore.Identity;
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

        public static void ConfigureIdentity(this IServiceCollection services)
        {
            var builder = services.AddIdentity<User, IdentityRole>(o =>
                {
                    o.Password.RequireDigit = true;
                    o.Password.RequireLowercase = true;
                    o.Password.RequireUppercase = true;
                    o.Password.RequireNonAlphanumeric = true;
                    o.Password.RequiredLength = 10;
                    o.User.RequireUniqueEmail = true;
                })
                .AddEntityFrameworkStores<IdentityRepositoryContext>()
                .AddDefaultTokenProviders();
        }
    }
}