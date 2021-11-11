using ApolloBus.Validation;

namespace ApolloBus.Clients.RabbitMQ.Model.Interfaces
{
    public interface IComplementaryConfigRabbit : IComplementaryConfig
    {
        string QueueName { get; set; }
        string BrokenName { get; set; }

    }
}
