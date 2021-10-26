using ApolloBus.InterfacesAbstraction;
using ApolloBus.RabbitMQ.Model;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using RabbitMQ.Client;
using Serilog;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
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

            ComplementaryConfig complementaryConfig = GetComplementaryConfigValues<ComplementaryConfig>(keyValuePairComplementaryConfig);

            string complementaryConfigValid = complementaryConfig.IsValid();
            if (complementaryConfigValid != string.Empty)
            {
                Log.Logger.Error(complementaryConfigValid);
                throw new Exception(complementaryConfigValid);
            }

            ConnectionFactory connectionFactory = GetComplementaryConfigValues<ConnectionFactory>(keyValuePairConfig);
            string connectionFactoryValidation = new ConnectionFactoryValidation(connectionFactory).IsValid();

            if (connectionFactoryValidation != string.Empty)
            {
                Log.Logger.Error(connectionFactoryValidation);
                throw new Exception(connectionFactoryValidation);
            }


            services.AddSingleton<ISubscriptionsManager, InMemorySubscriptionsManager>();
            services.AddSingleton(Log.Logger);
            services.AddSingleton<IRabbitMQConnection, RabbitMQConnection>(sp =>
            {
                return new RabbitMQConnection(connectionFactory, Log.Logger, complementaryConfig);
            });



            services.AddSingleton<IApolloBus, ApolloBusRabbitMQ>(sp =>
            {
                var logger = sp.GetRequiredService<ILogger>();
                var ApolloBusSubcriptionsManager = sp.GetRequiredService<ISubscriptionsManager>();
                var rabbitMQConnection = sp.GetRequiredService<IRabbitMQConnection>();
                var serviceProvider = sp.GetRequiredService<IServiceScopeFactory>();
                return new ApolloBusRabbitMQ(rabbitMQConnection, ApolloBusSubcriptionsManager, logger, serviceProvider, complementaryConfig);
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
