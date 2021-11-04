using ApolloBus.RabbitMQ.Model.Interfaces;
using ApolloBus.Validation;
using System;
using System.Collections.Generic;
using System.Text;

namespace ApolloBus.RabbitMQ.Model
{
    public sealed class ComplementaryConfigRabbit : IComplementaryConfigRabbit, IValid
    {
        public int Retry { get; set; } = 5;
        public string QueueName { get; set; }
        public string BrokenName { get; set; }

        public ComplementaryConfigRabbit(int retry, string queueName, string brokenName)
        {
            Retry = retry;
            QueueName = queueName;
            BrokenName = brokenName;
        }
        public ComplementaryConfigRabbit()
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
