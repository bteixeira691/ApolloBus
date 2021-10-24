using ApolloBus.Events;
using ApolloBus.InterfacesAbstraction;
using Azure.Messaging.ServiceBus;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using Serilog;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace ApolloBus.ServiceBus
{
    public class ApolloBusServiceBus : IApolloBus
    {
        private readonly ISubscriptionsManager _subsManager;
        private readonly ILogger _logger;
        private readonly ServiceBusConnection _serviceBusConnection;
        private readonly IServiceScopeFactory _serviceScopeFactory;

        public ApolloBusServiceBus(IServiceScopeFactory serviceScopeFactory, ISubscriptionsManager subscriptionsManager,
            ILogger logger, ServiceBusConnection serviceBusConnection)
        {
            _logger = logger;
            _serviceBusConnection = serviceBusConnection;
            _subsManager = subscriptionsManager;
            _serviceScopeFactory = serviceScopeFactory;
        }

        public async Task Publish(Event _event)
        {
            if (_event == null)
            {
                _logger.Warning("Event is null");
                return;
            }

            var eventName = _event.GetType().Name;

            try
            {
                var sender = await _serviceBusConnection.CreateSender();
                _logger.Information($"Publishing the event {_event} to ServiceBus topic {eventName}");
                
                var message = JsonConvert.SerializeObject(_event);

                ServiceBusMessage serviceBusMessage = new ServiceBusMessage(message);
                serviceBusMessage.Subject = eventName;

                await sender.SendMessageAsync(serviceBusMessage);

            }
            catch (Exception e)
            {
                _logger.Error($"Error occured during publishing the event to topic {_event}");
                _logger.Error($"Sender exception {e.Message}, StackTrace {e.StackTrace}");
            }
   
        }

        public async Task Subscribe<T, TH>()
            where T : Event
            where TH : IEventHandler<T>
        {
 
            var eventName = typeof(T).Name;
            var processor = await _serviceBusConnection.CreateProcessor();
            _subsManager.AddSubscription<T, TH>();


            processor.ProcessMessageAsync += MessageHandler;
            processor.ProcessErrorAsync += ErrorHandler;

            await processor.StartProcessingAsync();


        }

        private Task ErrorHandler(ProcessErrorEventArgs args)
        {
            _logger.Error($"ErrorHandler exception {args.Exception.Message}, StackTrace {args.Exception.StackTrace}");
            return Task.CompletedTask;
        }

        private async Task MessageHandler(ProcessMessageEventArgs args)
        {
            string body = args.Message.Body.ToString();
            await ProcessEvent(args.Message.Subject, body);

            await args.CompleteMessageAsync(args.Message);
        }

        private async Task ProcessEvent(string eventName, string message)
        {
            _logger.Information("Processing ServiceBus event: {EventName}", eventName);

            if (_subsManager.HasSubscriptionsForEvent(eventName))
            {
                using (var scope = _serviceScopeFactory.CreateScope())
                {
                    var subscriptions = _subsManager.GetHandlersForEvent(eventName);

                    foreach (var subscription in subscriptions)
                    {
                        var handler = scope.ServiceProvider.GetRequiredService(subscription.HandlerType);

                        if (handler == null)
                            continue;

                        var eventType = _subsManager.GetEventTypeByName(eventName);
                        var Event = JsonConvert.DeserializeObject(message, eventType);
                        var concreteType = typeof(IEventHandler<>).MakeGenericType(eventType);

                        await Task.Yield();
                        await (Task)concreteType.GetMethod("Handler").Invoke(handler, new object[] { Event });

                    }
                }
            }
            else
            {
                _logger.Warning("No subscription for ServiceBus event: {EventName}", eventName);
            }
        }

    }
}
