using ApolloBus.Clients.RabbitMQ.Model;
using ApolloBus.Clients.RabbitMQ.Model.Interfaces;
using ApolloBus.InterfacesAbstraction;
using ApolloBus.Polly;
using ApolloBus.StartupServices;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using RabbitMQ.Client;
using Serilog;
using System;


namespace ApolloBus.Clients.RabbitMQ
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

            services.AddSingleton<IComplementaryConfigRabbit, ComplementaryConfig>(sp =>
            {
                return new ComplementaryConfig(complementaryConfig.Retry, complementaryConfig.QueueName, complementaryConfig.BrokenName);
            });


            services.AddSingleton<IPollyPolicy, PollyPolicy>(sp =>
             {
                 var cConfig = sp.GetRequiredService<IComplementaryConfigRabbit>();
                 var logger = sp.GetRequiredService<ILogger>();
                 return new PollyPolicy(logger, cConfig);
             });

            services.AddSingleton<IRabbitMQConnection, RabbitMQConnection>(sp =>
            {
                var pollyPolicy = sp.GetRequiredService<IPollyPolicy>();
                return new RabbitMQConnection(connectionFactory, Log.Logger, pollyPolicy);
            });


            services.AddSingleton<IApolloBus, ApolloBusRabbitMQ>(sp =>
            {
                var logger = sp.GetRequiredService<ILogger>();
                var ApolloBusSubcriptionsManager = sp.GetRequiredService<ISubscriptionsManager>();
                var rabbitMQConnection = sp.GetRequiredService<IRabbitMQConnection>();
                var serviceProvider = sp.GetRequiredService<IServiceScopeFactory>();
                var pollyPolicy = sp.GetRequiredService<IPollyPolicy>();
                var ccRabbit = sp.GetRequiredService<IComplementaryConfigRabbit>();
                return new ApolloBusRabbitMQ(rabbitMQConnection, ApolloBusSubcriptionsManager, logger, serviceProvider, pollyPolicy, ccRabbit);
            });

            RegisterHandlers.AddHandlers(services);
            HangfireServices.AddHangfireServices(services, configuration);
        }
    }
}
