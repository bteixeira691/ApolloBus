using ApolloBus.Validation;
using System;
using System.Collections.Generic;
using System.Text;

namespace ApolloBus.RabbitMQ.Model.Interfaces
{
    public interface IComplementaryConfigRabbit : IComplementaryConfig
    {
        string QueueName { get; set; }
        string BrokenName { get; set; }

    }
}
