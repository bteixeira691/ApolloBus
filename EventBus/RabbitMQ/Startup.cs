using ApolloBus.InterfacesAbstraction;
using ApolloBus.RabbitMQ.Model;
using ApolloBus.StartupServices;
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

            ComplementaryConfig complementaryConfig = MappingConfigValues.GetMappingValues<ComplementaryConfig>(keyValuePairComplementaryConfig);

            string complementaryConfigValid = complementaryConfig.IsValid();
            if (complementaryConfigValid != string.Empty)
            {
                Log.Logger.Error(complementaryConfigValid);
                throw new Exception(complementaryConfigValid);
            }

            ConnectionFactory connectionFactory = MappingConfigValues.GetMappingValues<ConnectionFactory>(keyValuePairConfig);
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

            RegisterHandlers.AddHandlers(services);
        }
    }
}
