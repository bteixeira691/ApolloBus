using Confluent.Kafka;
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
            var keyValuePairProducer = configuration.GetSection("Kafka:ProducerConfig").GetChildren();
            var keyValuePairConsumer = configuration.GetSection("Kafka:ConsumerConfig").GetChildren();


            ProducerConfig producerConfig = MappingConfigValues.GetMappingValues<ProducerConfig>(keyValuePairProducer);
            string producerConfigValidation = new ClientConfigValidation(producerConfig).IsValid();
            if (producerConfigValidation != string.Empty)
            {
                Log.Logger.Error(producerConfigValidation);
                throw new Exception(producerConfigValidation);
            }

            ConsumerConfig consumerConfig = MappingConfigValues.GetMappingValues<ConsumerConfig>(keyValuePairConsumer);
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
