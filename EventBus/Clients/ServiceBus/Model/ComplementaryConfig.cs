using ApolloBus.Validation;

namespace ApolloBus.Clients.ServiceBus.Model
{
    public sealed class ComplementaryConfig : IComplementaryConfigServiceBus, IValid
    {
        public string QueueOrTopic { get; set; }
        public bool IsQueue { get; set; }
        public string ConnectionString { get; set; }
        public string SubscriptionName { get; set; }
        public int Retry { get; set; } = 5;

        public string IsValid()
        {
            if (string.IsNullOrWhiteSpace(ConnectionString) || string.IsNullOrEmpty(ConnectionString))
                return "ConnectionString is not valid!";

            if (IsQueue && (string.IsNullOrWhiteSpace(QueueOrTopic) || string.IsNullOrEmpty(QueueOrTopic)))
                return "QueueOrTopic is not valid!";

            if (!IsQueue)
                if ((string.IsNullOrWhiteSpace(QueueOrTopic) || string.IsNullOrEmpty(QueueOrTopic)) || (string.IsNullOrWhiteSpace(SubscriptionName) || string.IsNullOrEmpty(QueueOrTopic)))
                    return "QueueOrTopic/SubscriptionName are not valid!";

            return string.Empty;
        }
    }
}
