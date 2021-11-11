using Confluent.Kafka;
using Newtonsoft.Json;
using System;
using System.Text;

namespace ApolloBus.Clients.Kafka
{
    public class Deserializer<T> : IDeserializer<T>
    {

        T IDeserializer<T>.Deserialize(ReadOnlySpan<byte> data, bool isNull, SerializationContext context)
        {
            var dataString = Encoding.UTF8.GetString(data.ToArray());
            return JsonConvert.DeserializeObject<T>(dataString);
        }
    }
}
