using Hangfire;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace ApolloBus.StartupServices
{
    public static class HangfireServices
    {
        public static IServiceCollection AddHangfireServices(this IServiceCollection services)
        {
            services.AddHangfireServer();
            services.AddHangfire(config => config
                .UseIgnoredAssemblyVersionTypeResolver()
                .UseInMemoryStorage()
                .UseSerializerSettings(new JsonSerializerSettings
                {
                    TypeNameHandling = TypeNameHandling.Auto
                }));

            return services;
        }
    }
}
