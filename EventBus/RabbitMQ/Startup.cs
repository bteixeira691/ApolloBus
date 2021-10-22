using ApolloBus.InterfacesAbstraction;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using RabbitMQ.Client;
using Serilog;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;
using System.Text;

namespace ApolloBus.RabbitMQ
{
    public static class Startup
    {

        public static void AddRabbitMq(this IServiceCollection services, IConfiguration configuration)
        {
            var keyValuePairConfig = configuration.GetSection("RabbitMQ:Config").GetChildren();
            var keyValuePairComplementaryConfig = configuration.GetSection("RabbitMQ:ComplementaryConfig").GetChildren();

            ComplementaryConfig complementaryConfig = GetComplementaryConfigValues(keyValuePairComplementaryConfig);

            services.AddSingleton<ISubscriptionsManager, InMemorySubscriptionsManager>();
            services.AddSingleton(Log.Logger);
            services.AddSingleton<IRabbitMQConnection, RabbitMQConnection>(sp => 
            {
                return new RabbitMQConnection(GetConnectionValues(keyValuePairConfig),Log.Logger, complementaryConfig);
            });
           


            services.AddSingleton<IApolloBus, ApolloBusRabbitMQ>(sp =>
            {
                var logger = sp.GetRequiredService<ILogger>();
                var ApolloBusSubcriptionsManager = sp.GetRequiredService<ISubscriptionsManager>();
                var rabbitMQConnection = sp.GetRequiredService<IRabbitMQConnection>();
                var serviceProvider = sp.GetRequiredService<IServiceScopeFactory>();
                return new ApolloBusRabbitMQ(rabbitMQConnection,ApolloBusSubcriptionsManager, logger, serviceProvider, complementaryConfig);
            });



        }

        private static ConnectionFactory GetConnectionValues(IEnumerable<IConfigurationSection> children)
        {
            ConnectionFactory connection = new ConnectionFactory();
            Type type = typeof(ConnectionFactory);

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
                            propInfo.SetValue(connection, null, null);
                            break;
                        }
                        tProp = new NullableConverter(propInfo.PropertyType).UnderlyingType;
                    }
                    propInfo.SetValue(connection, Convert.ChangeType(child.Value, tProp), null);
                }
                catch (Exception e)
                {
                    Log.Error($"Property does not exist {child.Key}");
                    Log.Information(e.Message);

                }
            }
            return connection;
        }

        private static ComplementaryConfig GetComplementaryConfigValues(IEnumerable<IConfigurationSection> children)
        {
            ComplementaryConfig complementaryConfig = new ComplementaryConfig();
            Type type = typeof(ComplementaryConfig);

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
                            propInfo.SetValue(complementaryConfig, null, null);
                            break;
                        }
                        tProp = new NullableConverter(propInfo.PropertyType).UnderlyingType;
                    }
                    propInfo.SetValue(complementaryConfig, Convert.ChangeType(child.Value, tProp), null);
                }
                catch (Exception e)
                {
                    Log.Error($"Property does not exist {child.Key}");
                    Log.Information(e.Message);

                }
            }
            return complementaryConfig;
        }


    }
}
