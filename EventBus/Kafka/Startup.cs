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

namespace ApolloBus.Kafka
{
    public static class Startup
    {
      
        public static void AddKafka(this IServiceCollection services, IConfiguration configuration)
        {
            var keyValuePairProducer = configuration.GetSection("Kafka:ProducerConfig").GetChildren();
            var keyValuePairConsumer = configuration.GetSection("Kafka:ConsumerConfig").GetChildren();


            ProducerConfig producerConfig = GetComplementaryConfigValues<ProducerConfig>(keyValuePairProducer);
            string producerConfigValidation = new ClientConfigValidation(producerConfig).IsValid();
            if (producerConfigValidation != string.Empty)
            {
                Log.Logger.Error(producerConfigValidation);
                throw new Exception(producerConfigValidation);
            }

            ConsumerConfig consumerConfig = GetComplementaryConfigValues<ConsumerConfig>(keyValuePairConsumer);
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
