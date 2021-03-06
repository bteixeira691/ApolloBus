using ApolloBus.Clients.RabbitMQ.Model.Interfaces;
using ApolloBus.Validation;

namespace ApolloBus.Clients.RabbitMQ.Model
{
    public sealed class ComplementaryConfig : IComplementaryConfigRabbit, IValid
    {
        public int Retry { get; set; } = 5;
        public string QueueName { get; set; }
        public string BrokenName { get; set; }

        public ComplementaryConfig(int retry, string queueName, string brokenName)
        {
            Retry = retry;
            QueueName = queueName;
            BrokenName = brokenName;
        }
        public ComplementaryConfig()
        {

        }
        public string IsValid()
        {
            if (string.IsNullOrWhiteSpace(QueueName) || string.IsNullOrEmpty(QueueName))
                return "Your QueueName is not valid!";

            if (string.IsNullOrWhiteSpace(BrokenName) || string.IsNullOrEmpty(BrokenName))
                return "Your BrokenName is not valid!";

            return string.Empty;
        }

    }
}
