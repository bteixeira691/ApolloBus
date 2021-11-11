using ApolloBus.Clients.ServiceBus.Model;
using Azure.Messaging.ServiceBus;
using System;
using System.Threading.Tasks;

namespace ApolloBus.Clients.ServiceBus
{
    public class ServiceBusConnection
    {
        private readonly ComplementaryConfig _complementaryConfig;
        private readonly ServiceBusProcessorOptions _serviceBusProcessorOptions;
        private readonly ServiceBusClientOptions _serviceBusClientOptions;


        public ServiceBusConnection(ComplementaryConfig complementaryConfig, ServiceBusProcessorOptions serviceBusProcessorOptions,
            ServiceBusClientOptions serviceBusClientOptions)
        {
            _serviceBusClientOptions = serviceBusClientOptions;
            _complementaryConfig = complementaryConfig ?? throw new ArgumentNullException(nameof(complementaryConfig));
            _serviceBusProcessorOptions = serviceBusProcessorOptions;
        }


        public async Task<ServiceBusSender> CreateSender()
        {
            ServiceBusClient client = new ServiceBusClient(_complementaryConfig.ConnectionString);
            return client.CreateSender(_complementaryConfig.QueueOrTopic);

        }

        public async Task<ServiceBusProcessor> CreateProcessor()
        {
            ServiceBusClient client = new ServiceBusClient(_complementaryConfig.ConnectionString, _serviceBusClientOptions);
            if (_complementaryConfig.IsQueue)
                return client.CreateProcessor(_complementaryConfig.QueueOrTopic, _serviceBusProcessorOptions);
            return client.CreateProcessor(_complementaryConfig.QueueOrTopic, _complementaryConfig.SubscriptionName, _serviceBusProcessorOptions);
        }
    }
}
