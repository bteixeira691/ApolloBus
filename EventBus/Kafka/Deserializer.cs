using Confluent.Kafka;
using ApolloBus.Events;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace ApolloBus.Kafka
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
