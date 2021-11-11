using ApolloBus.Validation;
using RabbitMQ.Client;

namespace ApolloBus.Clients.RabbitMQ.Model
{
    public class ConnectionFactoryValidation : IValid
    {
        private ConnectionFactory ConnectionFactory { get; set; }

        public ConnectionFactoryValidation(ConnectionFactory connectionFactory)
        {
            ConnectionFactory = connectionFactory;
        }

        public string IsValid()
        {
            if (string.IsNullOrEmpty(ConnectionFactory.HostName) || string.IsNullOrEmpty(ConnectionFactory.HostName))
                return "HostName is not valid!";

            if (string.IsNullOrEmpty(ConnectionFactory.UserName) || string.IsNullOrEmpty(ConnectionFactory.UserName))
                return "UserName is not valid!";

            if (string.IsNullOrEmpty(ConnectionFactory.Password) || string.IsNullOrEmpty(ConnectionFactory.Password))
                return "Password is not valid!";

            if (string.IsNullOrEmpty(ConnectionFactory.VirtualHost) || string.IsNullOrEmpty(ConnectionFactory.VirtualHost))
                return "VirtualHost is not valid!";

            return string.Empty;
        }
    }
}
