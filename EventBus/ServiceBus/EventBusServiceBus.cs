using ApolloBus.Events;
using ApolloBus.InterfacesAbstraction;
using ApolloBus.Polly;
using Azure.Messaging.ServiceBus;
using Hangfire;
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
        private readonly IPollyPolicy _pollyPolicy;

        public ApolloBusServiceBus(IServiceScopeFactory serviceScopeFactory, ISubscriptionsManager subscriptionsManager,
            ILogger logger, IPollyPolicy pollyPolicy, ServiceBusConnection serviceBusConnection)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));

            _pollyPolicy = pollyPolicy ?? throw new ArgumentNullException(nameof(pollyPolicy));
            _serviceBusConnection = serviceBusConnection ?? throw new ArgumentNullException(nameof(serviceBusConnection));
            _subsManager = subscriptionsManager ?? throw new ArgumentNullException(nameof(subscriptionsManager));
            _serviceScopeFactory = serviceScopeFactory ?? throw new ArgumentNullException(nameof(serviceScopeFactory));
        }

        public async Task Publish(ApolloEvent _event)
        {
            if (_event == null)
            {
                _logger.Warning("Event is null");
                return;
            }

            var eventName = _event.GetType().Name;

            var policy = _pollyPolicy.ApolloRetryPolicyEvent(_event.Id);

            await policy.ExecuteAsync(async () =>
            {
                var sender = await _serviceBusConnection.CreateSender();
                _logger.Information($"Publishing the event {_event} to ServiceBus -> {eventName}");

                var message = JsonConvert.SerializeObject(_event);

                ServiceBusMessage serviceBusMessage = new ServiceBusMessage(message);
                serviceBusMessage.Subject = eventName;

                await sender.SendMessageAsync(serviceBusMessage);
            });
        }

        public async Task Subscribe<T, TH>()
            where T : ApolloEvent
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
            _logger.Information("Processing ServiceBus event: {eventName}", eventName);

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
                _logger.Warning("No subscription for ServiceBus event: {eventName}", eventName);
            }
        }

        public async Task PublishRecurring(ApolloEvent _event, string CronExpressions)
        {
            try
            {
                _logger.Information($"Recurring Publish with event {_event}");
                RecurringJob.AddOrUpdate("PublishApolloEvent", () => Publish(_event), CronExpressions);

            }
            catch (Exception e)
            {
                _logger.Error(e, $"Error PublishRecurring {_event}, CronExpression {CronExpressions}");
            }
        }
        public async Task RemovePublishRecurring()
        {
            try
            {
                RecurringJob.RemoveIfExists("PublishApolloEvent");

            }
            catch (Exception e)
            {
                _logger.Error(e, $"Error RemovePublishRecurring with JobId PublishApolloEvent");
            }
        }

        public async Task PublishDelay(ApolloEvent _event, int seconds)
        {
            try
            {
                _logger.Information($"Delay Publish with event {_event}, delay time {seconds}seconds");
                BackgroundJob.Schedule(() => Publish(_event), TimeSpan.FromSeconds(seconds));

            }
            catch (Exception e)
            {
                _logger.Error(e, $"Error PublishDelay {_event}, delay time {seconds}seconds");
            }
        }

    }
}
