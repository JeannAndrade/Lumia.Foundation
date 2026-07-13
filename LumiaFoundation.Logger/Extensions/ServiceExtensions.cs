using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LumiaFoundation.Logger.Contracts;
using LumiaFoundation.Logger.LoggerService;
using Microsoft.Extensions.DependencyInjection;

namespace LumiaFoundation.Logger.Extensions
{
    public static class ServiceExtensions
    {
        public static void ConfigureLoggerService(this IServiceCollection services) =>
            services.AddSingleton<ILoggerManager, LoggerManager>();
    }
}