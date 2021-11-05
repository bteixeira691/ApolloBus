using ApolloBus.Validation;
using System;
using System.Collections.Generic;
using System.Text;

namespace ApolloBus.ServiceBus.Model
{
    public interface IComplementaryConfigServiceBus : IComplementaryConfig
    {
        string QueueOrTopic { get; set; }
        bool IsQueue { get; set; }
        string ConnectionString { get; set; }
        string SubscriptionName { get; set; }
    }
}
