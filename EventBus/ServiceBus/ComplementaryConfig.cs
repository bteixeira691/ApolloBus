using System;
using System.Collections.Generic;
using System.Text;

namespace ApolloBus.ServiceBus
{
    public class ComplementaryConfig
    {
        public string QueueOrTopic { get; set; }
        public bool IsQueue{ get; set; }
        public string ConnectionString { get; set; }
        public string SubscriptionName { get; set; }

        public bool IsValid()
        {
            if (IsQueue && (string.IsNullOrWhiteSpace(QueueOrTopic) || string.IsNullOrEmpty(QueueOrTopic)))
                return false;

            if (!IsQueue && (string.IsNullOrWhiteSpace(QueueOrTopic)|| string.IsNullOrEmpty(QueueOrTopic)) || (string.IsNullOrWhiteSpace(SubscriptionName) || string.IsNullOrEmpty(QueueOrTopic)))
                return false;

                return true;
        }
    }    
}
