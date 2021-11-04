using ApolloBus.Kafka.Model.Interfaces;
using ApolloBus.Validation;
using System;
using System.Collections.Generic;
using System.Text;

namespace ApolloBus.Kafka.Model
{
    public sealed class ComplementaryConfig : IComplementaryConfigKafka
    {
        public int Retry { get; set; } = 5;
    }
}
