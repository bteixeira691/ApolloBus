using ApolloBus.Clients.Kafka.Model.Interfaces;

namespace ApolloBus.Clients.Kafka.Model
{
    public sealed class ComplementaryConfig : IComplementaryConfigKafka
    {
        public int Retry { get; set; } = 5;
    }
}
