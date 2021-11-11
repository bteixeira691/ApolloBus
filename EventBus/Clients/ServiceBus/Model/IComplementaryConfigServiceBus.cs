using ApolloBus.Validation;

namespace ApolloBus.Clients.ServiceBus.Model
{
    public interface IComplementaryConfigServiceBus : IComplementaryConfig
    {
        string QueueOrTopic { get; set; }
        bool IsQueue { get; set; }
        string ConnectionString { get; set; }
        string SubscriptionName { get; set; }
    }
}
