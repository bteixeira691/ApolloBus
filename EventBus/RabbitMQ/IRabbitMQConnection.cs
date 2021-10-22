using RabbitMQ.Client;
using System;

namespace ApolloBus.RabbitMQ
{
    public interface IRabbitMQConnection : IDisposable
    {
        bool IsConnected { get; }

        bool TryConnect();

        IModel CreateModel();
    }
}
