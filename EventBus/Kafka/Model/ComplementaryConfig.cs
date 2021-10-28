using ApolloBus.Validation;
using System;
using System.Collections.Generic;
using System.Text;

namespace ApolloBus.Kafka.Model
{
    public class ComplementaryConfig : IComplementaryConfig
    {
        public int Retry { get; set; } = 5;
    }
}
