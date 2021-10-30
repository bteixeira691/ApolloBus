using ApolloBus.InterfacesAbstraction;
using ApolloBus.Polly;
using ApolloBus.RabbitMQ.Model;
using ApolloBus.RabbitMQ.Model.Interfaces;
using ApolloBus.StartupServices;
using ApolloBus.Validation;
using Hangfire;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
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
            ComplementaryConfigRabbit complementaryConfig = configuration.GetSection("RabbitMQ:ComplementaryConfig").Get<ComplementaryConfigRabbit>();


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

            services.AddSingleton<IComplementaryConfigRabbit, ComplementaryConfigRabbit>(sp=> 
            {
                return new ComplementaryConfigRabbit(complementaryConfig.Retry, complementaryConfig.QueueName, complementaryConfig.BrokenName);
            });


            services.AddSingleton<IPollyPolicy,PollyPolicy>(sp =>
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
