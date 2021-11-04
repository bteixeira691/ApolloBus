using ApolloBus.AmazonSQS.Model.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace ApolloBus.AmazonSQS.Model
{
    public class ComplementaryConfig : IComplementaryConfigAmazonSQS
    {
        public int Retry { get; set; } = 5;
        public string QueueName { get; set; }
    }
}
