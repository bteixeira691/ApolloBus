using Confluent.Kafka;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace ApolloBus.Kafka
{
    public class Serializer<T> : ISerializer<T> 
    {
       
        public byte[] Serialize(T data, SerializationContext context)
        {
            var message = JsonConvert.SerializeObject(data);
            return Encoding.UTF8.GetBytes(message);
        }
    }
}
