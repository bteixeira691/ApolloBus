using ApolloBus.InterfacesAbstraction;
using ApolloBus.ServiceBus.Model;
using ApolloBus.StartupServices;
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

            var keyValuePairComplementaryConfig = configuration.GetSection("ServiceBus:ComplementaryConfig").GetChildren();
            var keyValuePairserviceBusProcessorOptions = configuration.GetSection("ServiceBus:ServiceBusProcessorOptions").GetChildren();

            ServiceBusProcessorOptions serviceBusProcessorOptions = MappingConfigValues.GetMappingValues<ServiceBusProcessorOptions>(keyValuePairserviceBusProcessorOptions);
            ComplementaryConfig complementaryConfig = MappingConfigValues.GetMappingValues<ComplementaryConfig>(keyValuePairComplementaryConfig);
            string complementaryConfigValid = complementaryConfig.IsValid();
            if (complementaryConfigValid != string.Empty)
            {
                Log.Logger.Error(complementaryConfigValid);
                throw new Exception(complementaryConfigValid);
            }

            services.AddSingleton<ISubscriptionsManager, InMemorySubscriptionsManager>();
            services.AddSingleton(Log.Logger);

            services.AddSingleton(new ServiceBusConnection(complementaryConfig, serviceBusProcessorOptions));
            services.AddSingleton<IApolloBus, ApolloBusServiceBus>(sp =>
            {
                var serviceBusConnection = sp.GetRequiredService<ServiceBusConnection>();
                var logger = sp.GetRequiredService<ILogger>();
                var subcriptionsManager = sp.GetRequiredService<ISubscriptionsManager>();
                var serviceProvider = sp.GetRequiredService<IServiceScopeFactory>();
                return new ApolloBusServiceBus(serviceProvider, subcriptionsManager, logger, serviceBusConnection);
            });

            RegisterHandlers.AddHandlers(services);
        }
    }
}
