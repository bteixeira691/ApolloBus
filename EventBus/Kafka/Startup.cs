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

namespace ApolloBus.Kafka
{
    public static class Startup
    {
      
        public static void AddKafka(this IServiceCollection services, IConfiguration configuration)
        {
            var keyValuePairProducer = configuration.GetSection("Kafka:ProducerConfig").GetChildren();
            var keyValuePairConsumer = configuration.GetSection("Kafka:ConsumerConfig").GetChildren();

            services.AddSingleton<ISubscriptionsManager, InMemorySubscriptionsManager>();

            services.AddSingleton(Log.Logger);

            services.AddSingleton(new KafkaConnection(GetProducerValues(keyValuePairProducer), GetComsumerValues(keyValuePairConsumer)));

            services.AddSingleton<IApolloBus, ApolloBusKafka>(sp =>
            {
                var kafkaConnection = sp.GetRequiredService<KafkaConnection>();
                var logger = sp.GetRequiredService<ILogger>();
                var ApolloBusSubcriptionsManager = sp.GetRequiredService<ISubscriptionsManager>();
                var serviceProvider = sp.GetRequiredService<IServiceScopeFactory>();
                return new ApolloBusKafka(ApolloBusSubcriptionsManager, logger, kafkaConnection, serviceProvider);
            });


        }

        private static ProducerConfig GetProducerValues(IEnumerable<IConfigurationSection> children)
        {
            ProducerConfig producer = new ProducerConfig();
            Type type = typeof(ProducerConfig);

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
                            propInfo.SetValue(producer, null, null);
                            break;
                        }
                        tProp = new NullableConverter(propInfo.PropertyType).UnderlyingType;
                    }
                    propInfo.SetValue(producer, Convert.ChangeType(child.Value, tProp), null);
                }
                catch(Exception e)
                {
                    Log.Error($"Property does not exist {child.Key}");
                    Log.Information(e.Message);

                }
            }
            return producer;
        }

        private static ConsumerConfig GetComsumerValues(IEnumerable<IConfigurationSection> children)
        {
            ConsumerConfig consumer = new ConsumerConfig();

            Type type = typeof(ConsumerConfig);

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
                            propInfo.SetValue(consumer, null, null);
                            break;
                        }
                        tProp = new NullableConverter(propInfo.PropertyType).UnderlyingType;
                    }
                    propInfo.SetValue(consumer, Convert.ChangeType(child.Value, tProp), null);
                }
                catch (Exception e)
                {
                    Log.Error($"Property does not exist {child.Key}");
                    Log.Information(e.Message);

                }
            }
            return consumer;

        }
    }
}
