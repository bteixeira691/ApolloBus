using Confluent.Kafka;
using System;

namespace ApolloBus.Clients.Kafka
{
    public class KafkaConnection
    {
        private readonly ProducerConfig _producerConfiguration;
        private readonly ConsumerConfig _consumerConfiguration;


        public KafkaConnection(ProducerConfig producerConfig, ConsumerConfig consumerConfig)
        {


            _producerConfiguration = producerConfig ?? throw new ArgumentNullException(nameof(producerConfig));
            _consumerConfiguration = consumerConfig ?? throw new ArgumentNullException(nameof(consumerConfig));


        }

        public IProducer<Null, T> ProducerBuilder<T>()
        {
            var _producerBuilder = new ProducerBuilder<Null, T>(_producerConfiguration)
                          .SetValueSerializer(new Serializer<T>())
                         .Build();

            return _producerBuilder;
        }

        public IConsumer<Null, T> ConsumerBuilder<T>()
        {
            var consumer = new ConsumerBuilder<Null, T>(_consumerConfiguration)
                .SetValueDeserializer(new Deserializer<T>())
                .Build();

            return consumer;
        }


    }
}
