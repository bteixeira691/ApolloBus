using Confluent.Kafka;
using Newtonsoft.Json;
using System.Text;

namespace ApolloBus.Clients.Kafka
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
