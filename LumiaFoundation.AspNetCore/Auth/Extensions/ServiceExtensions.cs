using System.Text;
using LumiaFoundation.AspNetCore.Auth.Identity.Model;
using LumiaFoundation.AspNetCore.Auth.Persistence;
using LumiaFoundation.AspNetCore.Auth.Utils;
using LumiaFoundation.AspNetCore.Commons.Config;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;

namespace LumiaFoundation.AspNetCore.Auth.Extensions
{
    public static class ServiceExtensions
    {
        public static void ConfigureIdentityMariaDbDatabase<T>(this IServiceCollection services, MariaDbConnectionHelper dbConfig, string migrationAssembly) where T : DbContext
        {
            var serverVersion = new MariaDbServerVersion(new Version(dbConfig.MajorVersion, dbConfig.MinorVersion, dbConfig.BuildVersion));

            services.AddDbContext<T>(options =>
                options
                .UseMySql(dbConfig.GetConectionString(), serverVersion, b => b.MigrationsAssembly(migrationAssembly)));
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
                .AddEntityFrameworkStores<IdentityContext>()
                .AddDefaultTokenProviders();
        }

        public static void ConfigureJWT(this IServiceCollection services, IAppConfigurationParameter configuration)
        {
            services.AddAuthentication(opt =>
            {
                opt.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                opt.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            }).AddJwtBearer(options =>
                {
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuer = true,
                        ValidateAudience = true,
                        ValidateLifetime = true,
                        ValidateIssuerSigningKey = true,
                        ValidIssuer = configuration.JwtParameter.JwtValidIssuer,
                        ValidAudience = configuration.JwtParameter.JwtValidAudience,
                        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration.JwtParameter.JwtSecret))
                    };
                });
        }
    }
}