using ApolloBus.InterfacesAbstraction;
using ApolloBus.Polly;
using ApolloBus.RabbitMQ.Model;
using ApolloBus.StartupServices;
using ApolloBus.Validation;
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
            ConnectionFactory connectionFactory = configuration.GetSection("RabbitMQ:ConnectionFactory").Get<ConnectionFactory>();
            ComplementaryConfig complementaryConfig = configuration.GetSection("RabbitMQ:ComplementaryConfig").Get<ComplementaryConfig>();


            string complementaryConfigValid = complementaryConfig.IsValid();
            if (complementaryConfigValid != string.Empty)
            {
                Log.Logger.Error(complementaryConfigValid);
                throw new Exception(complementaryConfigValid);
            }


            string connectionFactoryValidation = new ConnectionFactoryValidation(connectionFactory).IsValid();
            if (connectionFactoryValidation != string.Empty)
            {
                Log.Logger.Error(connectionFactoryValidation);
                throw new Exception(connectionFactoryValidation);
            }


            services.AddSingleton<ISubscriptionsManager, InMemorySubscriptionsManager>();
            services.AddSingleton(Log.Logger);
            services.AddSingleton<IComplementaryConfig, ComplementaryConfig>();



            services.AddSingleton<IPollyPolicy,PollyPolicy>(sp =>
            {
                var cConfig = sp.GetRequiredService<IComplementaryConfig>();
                var logger = sp.GetRequiredService<ILogger>();
                return new PollyPolicy(logger, cConfig);
            });

            services.AddSingleton<IRabbitMQConnection, RabbitMQConnection>(sp =>
            {
                var pollyPolicy = sp.GetRequiredService<IPollyPolicy>();
                return new RabbitMQConnection(connectionFactory, Log.Logger, pollyPolicy,complementaryConfig);
            });


            services.AddSingleton<IApolloBus, ApolloBusRabbitMQ>(sp =>
            {
                var logger = sp.GetRequiredService<ILogger>();
                var ApolloBusSubcriptionsManager = sp.GetRequiredService<ISubscriptionsManager>();
                var rabbitMQConnection = sp.GetRequiredService<IRabbitMQConnection>();
                var serviceProvider = sp.GetRequiredService<IServiceScopeFactory>();
                var pollyPolicy = sp.GetRequiredService<IPollyPolicy>();
                return new ApolloBusRabbitMQ(rabbitMQConnection, ApolloBusSubcriptionsManager, logger, serviceProvider, pollyPolicy, complementaryConfig);
            });

            RegisterHandlers.AddHandlers(services);
        }
    }
}
