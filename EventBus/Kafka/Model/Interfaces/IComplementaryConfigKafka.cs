using System;
using System.Collections.Generic;
using System.Text;

namespace ApolloBus.Kafka.Model.Interfaces
{
    public interface IComplementaryConfigKafka
    {
        int Retry { get; set; }
    }
}
