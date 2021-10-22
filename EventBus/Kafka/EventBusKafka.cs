using Confluent.Kafka;
using ApolloBus.Events;
using ApolloBus.InterfacesAbstraction;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading.Tasks;
using System.Linq;
using Serilog;

namespace ApolloBus.Kafka
{
    public class ApolloBusKafka : IApolloBus
    {

        private readonly ISubscriptionsManager _subscriptionManager;
        private readonly ILogger _logger;
        private readonly KafkaConnection _kafkaConnection;
        private readonly IServiceScopeFactory _serviceScopeFactory;



        public ApolloBusKafka(ISubscriptionsManager subscriptionManager, ILogger logger,
            KafkaConnection kafkaConnection, IServiceScopeFactory serviceProvider)
        {

            _subscriptionManager = subscriptionManager ?? throw new ArgumentNullException(nameof(subscriptionManager));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _kafkaConnection = kafkaConnection ?? throw new ArgumentNullException(nameof(kafkaConnection));
            _serviceScopeFactory = serviceProvider ?? throw new ArgumentException($"Cannot resolve IServiceScopeFactory from {nameof(serviceProvider)}");
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
                var producer = _kafkaConnection.ProducerBuilder<Event>();

                _logger.Information($"Publishing the event {_event} to Kafka topic {eventName}");
                var producerResult = await producer.ProduceAsync(eventName, new Message<Null, Event>() { Value = _event });

            }
            catch (Exception e)
            {
                _logger.Error($"Error occured during publishing the event to topic {_event}");
                _logger.Error($"Producer exception {e.Message}, StackTrace {e.StackTrace}");
            }
        }


        public async Task Subscribe<T, TH>()
            where T : Event
            where TH : IEventHandler<T>
        {
            var eventName = typeof(T).Name;

            using (var consumer = _kafkaConnection.ConsumerBuilder<T>())
            {
                //subscribe the handler to the event
                _subscriptionManager.AddSubscription<T, TH>();

                consumer.Subscribe(eventName);

                //create a task to listen to the topic
                await Task.Run(async () =>
                {
                    while (true)
                    {
                        try
                        {
                            _logger.Information($"Consuming from topic {eventName}");

                            var consumerResult = consumer.Consume();
                            await ProcessEvent(consumerResult.Message.Value);
                        }
                        catch (ConsumeException e)
                        {
                            _logger.Error($"Error `{e.Error.Reason}` occured during consuming the event from topic {eventName}");
                            _logger.Error($"Consume exception {e.Message}, StackTrace {e.StackTrace}");

                        }
                        catch (Exception e)
                        {
                            _logger.Error($"Error `{e.Message}` occured during consuming the event from topic {eventName}");
                        }
                    }
                });
            }
        }

        private async Task ProcessEvent<T>(T message) where T : Event
        {


            var eventName = message.GetType().Name;

            _logger.Information($"Process Event {eventName}");
            try
            {
                if (_subscriptionManager.HasSubscriptionsForEvent(eventName))
                {
                    using (var scope = _serviceScopeFactory.CreateScope())
                    {
                        var subscriptions = _subscriptionManager.GetHandlersForEvent(eventName);

                        _logger.Information($"Subscriptions number {subscriptions.Count()}");

                        foreach (var subscription in subscriptions)
                        {
                            var handler = scope.ServiceProvider.GetRequiredService(subscription.HandlerType);

                            if (handler == null)
                                continue;

                            var eventType = _subscriptionManager.GetEventTypeByName(eventName);
                            var concreteType = typeof(IEventHandler<>).MakeGenericType(eventType);


                            await Task.Yield();
                            await (Task)concreteType.GetMethod("Handler").Invoke(handler, new object[] { message });
                        }
                    }
                }
            }catch(Exception e)
            {

                _logger.Error($"Process Event has an error {e.Message}, StackTrace {e.StackTrace}");
            }
        }


    }
}

