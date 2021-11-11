using ApolloBus.Clients.AmazonSQS.Model.Interfaces;

namespace ApolloBus.Clients.AmazonSQS.Model
{
    public class ComplementaryConfig : IComplementaryConfigAmazonSQS
    {
        public int Retry { get; set; } = 5;
        public string QueueName { get; set; }
    }
}
