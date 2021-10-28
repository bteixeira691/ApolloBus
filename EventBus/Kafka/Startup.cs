﻿using Confluent.Kafka;
using ApolloBus.InterfacesAbstraction;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;
using System.Text;
using ApolloBus.Kafka.Model;
using ApolloBus.StartupServices;

namespace ApolloBus.Kafka
{
    public static class Startup
    {
      
        public static void AddKafka(this IServiceCollection services, IConfiguration configuration)
        {
            ProducerConfig producerConfig = configuration.GetSection("Kafka:ProducerConfig").Get<ProducerConfig>();
            ConsumerConfig consumerConfig = configuration.GetSection("Kafka:ConsumerConfig").Get<ConsumerConfig>();

            string producerConfigValidation = new ClientConfigValidation(producerConfig).IsValid();
            if (producerConfigValidation != string.Empty)
            {
                Log.Logger.Error(producerConfigValidation);
                throw new Exception(producerConfigValidation);
            }

            string consumerConfigValidation = new ClientConfigValidation(consumerConfig).IsValid();
            if (consumerConfigValidation != string.Empty)
            {
                Log.Logger.Error(consumerConfigValidation);
                throw new Exception(consumerConfigValidation);
            }

            services.AddSingleton<ISubscriptionsManager, InMemorySubscriptionsManager>();
            services.AddSingleton(Log.Logger);

            services.AddSingleton(new KafkaConnection(producerConfig, consumerConfig));
            services.AddSingleton<IApolloBus, ApolloBusKafka>(sp =>
            {
                var kafkaConnection = sp.GetRequiredService<KafkaConnection>();
                var logger = sp.GetRequiredService<ILogger>();
                var ApolloBusSubcriptionsManager = sp.GetRequiredService<ISubscriptionsManager>();
                var serviceProvider = sp.GetRequiredService<IServiceScopeFactory>();
                return new ApolloBusKafka(ApolloBusSubcriptionsManager, logger, kafkaConnection, serviceProvider);
            });

            RegisterHandlers.AddHandlers(services);
        }
    }
}
