using ApolloBus.Validation;
using Confluent.Kafka;

namespace ApolloBus.Clients.Kafka.Model
{
    public class ClientConfigValidation : IValid
    {
        private ClientConfig ClientConfig { get; set; }

        public ClientConfigValidation(ClientConfig clientConfig)
        {
            ClientConfig = clientConfig;
        }
        public string IsValid()
        {
            if (string.IsNullOrEmpty(ClientConfig.BootstrapServers) || string.IsNullOrWhiteSpace(ClientConfig.BootstrapServers))
                return "BootstrapServers is not valid!";

            return string.Empty;
        }
    }
}
