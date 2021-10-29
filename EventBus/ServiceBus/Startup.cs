using ApolloBus.InterfacesAbstraction;
using ApolloBus.Polly;
using ApolloBus.ServiceBus.Model;
using ApolloBus.StartupServices;
using ApolloBus.Validation;
using Azure.Messaging.ServiceBus;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;
using System.Text;

namespace ApolloBus.ServiceBus
{
    public static class Startup
    {

        public static void AddServiceBus(this IServiceCollection services, IConfiguration configuration)
        {

            ServiceBusClientOptions serviceBusClientOptions = configuration.GetSection("ServiceBus:ServiceBusClientOptions").Get<ServiceBusClientOptions>();
            ComplementaryConfig complementaryConfig = configuration.GetSection("ServiceBus:ComplementaryConfig").Get<ComplementaryConfig>();
            ServiceBusProcessorOptions serviceBusProcessorOptions = configuration.GetSection("ServiceBus:ServiceBusProcessorOptions").Get<ServiceBusProcessorOptions>();


            string complementaryConfigValid = complementaryConfig.IsValid();
            if (complementaryConfigValid != string.Empty)
            {
                Log.Logger.Error(complementaryConfigValid);
                throw new Exception(complementaryConfigValid);
            }


            services.AddSingleton<ISubscriptionsManager, InMemorySubscriptionsManager>();
            services.AddSingleton(Log.Logger);
            services.AddSingleton<IComplementaryConfigServiceBus, ComplementaryConfig>();

            services.AddSingleton<IPollyPolicy, PollyPolicy>(sp =>
            {
                var cConfig = sp.GetRequiredService<IComplementaryConfigServiceBus>();
                var logger = sp.GetRequiredService<ILogger>();
                return new PollyPolicy(logger, cConfig);
            });


            services.AddSingleton(new ServiceBusConnection(complementaryConfig, serviceBusProcessorOptions, serviceBusClientOptions));
            services.AddSingleton<IApolloBus, ApolloBusServiceBus>(sp =>
            {
                var serviceBusConnection = sp.GetRequiredService<ServiceBusConnection>();
                var logger = sp.GetRequiredService<ILogger>();
                var subcriptionsManager = sp.GetRequiredService<ISubscriptionsManager>();
                var serviceProvider = sp.GetRequiredService<IServiceScopeFactory>();
                var polly = sp.GetRequiredService<IPollyPolicy>();
                return new ApolloBusServiceBus(serviceProvider, subcriptionsManager, logger, polly,serviceBusConnection);
            });

            RegisterHandlers.AddHandlers(services);
            HangfireServices.AddHangfireServices(services);
        }
    }
}
