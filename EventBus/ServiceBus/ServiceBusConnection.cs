﻿using Azure.Messaging.ServiceBus;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace ApolloBus.ServiceBus
{
    public class ServiceBusConnection
    {
        private readonly ComplementaryConfig _complementaryConfig;
        private readonly ServiceBusProcessorOptions _serviceBusProcessorOptions;

        public ServiceBusConnection(ComplementaryConfig complementaryConfig, ServiceBusProcessorOptions serviceBusProcessorOptions)
        {
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
            ServiceBusClient client = new ServiceBusClient(_complementaryConfig.ConnectionString);
            if(_complementaryConfig.IsQueue)
                return client.CreateProcessor(_complementaryConfig.QueueOrTopic, _serviceBusProcessorOptions);
            return client.CreateProcessor(_complementaryConfig.QueueOrTopic, _complementaryConfig.SubscriptionName, _serviceBusProcessorOptions);
        }
    }
}
