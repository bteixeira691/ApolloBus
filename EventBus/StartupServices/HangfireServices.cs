using Hangfire;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;
using ApolloBus.Common.Model;

namespace ApolloBus.StartupServices
{
    public static class HangfireServices
    {
        public static IServiceCollection AddHangfireServices(this IServiceCollection services, IConfiguration configuration)
        {

            HangfireConfig hangfire = configuration.GetSection("HangfireConfig").Get<HangfireConfig>();
            if (hangfire != null)
                if (hangfire.UseHangfire==true)
                {
                    services.AddHangfireServer();
                    services.AddHangfire(config => config
                        .UseIgnoredAssemblyVersionTypeResolver()
                        .UseInMemoryStorage()
                        .UseSerializerSettings(new JsonSerializerSettings
                        {
                            TypeNameHandling = TypeNameHandling.Auto
                        }));
                }

            return services;
        }
    }
}
