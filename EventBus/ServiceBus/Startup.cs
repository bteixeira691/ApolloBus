using ApolloBus.InterfacesAbstraction;
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

            ComplementaryConfig complementaryConfig = GetComplementaryConfigValues<ComplementaryConfig>(keyValuePairComplementaryConfig);
            if (!complementaryConfig.IsValid())
            {
                Log.Logger.Error($"Error with ComplementaryConfig check your configuration {complementaryConfig}");
                throw new Exception("Error with ComplementaryConfig check your configuration/logs");
            }


            ServiceBusProcessorOptions serviceBusProcessorOptions = GetComplementaryConfigValues<ServiceBusProcessorOptions>(keyValuePairserviceBusProcessorOptions);

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


        }

        private static T GetComplementaryConfigValues<T>(IEnumerable<IConfigurationSection> children) where T : new()
        {
            T obj = new T();
            Type type = typeof(T);

            foreach (var child in children)
            {
                try
                {
                    PropertyInfo propInfo = type.GetProperty(child.Key);
                    Type tProp = propInfo.PropertyType;

                    if (tProp.IsGenericType && tProp.GetGenericTypeDefinition().Equals(typeof(Nullable<>)))
                    {
                        if (child.Value == null)
                        {
                            propInfo.SetValue(obj, null, null);
                            break;
                        }
                        tProp = new NullableConverter(propInfo.PropertyType).UnderlyingType;
                    }
                    propInfo.SetValue(obj, Convert.ChangeType(child.Value, tProp), null);
                }
                catch (Exception e)
                {
                    Log.Error($"Property does not exist {child.Key}");
                    Log.Information(e.Message);

                }
            }
            return obj;
        }

    }
}
