using Amazon.SQS;
using Amazon.SQS.Model;
using ApolloBus.Events;
using ApolloBus.InterfacesAbstraction;
using Hangfire;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApolloBus.AmazonSQS
{
    public class ApolloBusAmazonSQS : IApolloBus
    {
        private readonly ISubscriptionsManager _subsManager;
        private readonly ILogger _logger;
        private readonly AmazonSQSConnection _amazonSQSConnection;
        private readonly IServiceScopeFactory _serviceScopeFactory;
        private readonly IAmazonSQS _sqs;


        public ApolloBusAmazonSQS(IServiceScopeFactory serviceScopeFactory, ISubscriptionsManager subscriptionsManager, ILogger logger, AmazonSQSConnection amazonSQSConnection
            , IAmazonSQS sqs)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));

            _subsManager = subscriptionsManager ?? throw new ArgumentNullException(nameof(subscriptionsManager));
            _serviceScopeFactory = serviceScopeFactory ?? throw new ArgumentNullException(nameof(serviceScopeFactory));

            _amazonSQSConnection = amazonSQSConnection;
            _sqs = sqs;
        }
        public async Task Publish(ApolloEvent _event)
        {
            if (_event == null)
            {
                _logger.Warning("Event is null");
                return;
            }

            try
            {
                string message = JsonConvert.SerializeObject(_event);
                var sendRequest = new SendMessageRequest(await _amazonSQSConnection.GetCreateQueue(), message);
                sendRequest.MessageAttributes = new Dictionary<string, MessageAttributeValue>();
                sendRequest.MessageAttributes.Add("eventName", new MessageAttributeValue
                {
                    DataType = "String",
                    StringValue = _event.GetType().Name
                });

                _logger.Information($"Publishing the event {_event}");
                var sendResult = await _sqs.SendMessageAsync(sendRequest);

                if (sendResult.HttpStatusCode != System.Net.HttpStatusCode.OK)
                    throw new Exception("Message could not be send");
            }

            catch (Exception e)
            {
                _logger.Error($"Error occured during publishing the event {_event}");
                _logger.Error($"Sender exception {e.Message}, StackTrace {e.StackTrace}");
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

        public async Task Subscribe<T, TH>()
            where T : ApolloEvent
            where TH : IEventHandler<T>
        {

            var eventName = typeof(T).Name;

            _subsManager.AddSubscription<T, TH>();

            await ReceiveMessageAsync();
        }

        private async Task ReceiveMessageAsync()
        {
            var request = new ReceiveMessageRequest
            {
                QueueUrl = await _amazonSQSConnection.GetCreateQueue(),
                WaitTimeSeconds = 0,
                MaxNumberOfMessages = 10,
                MessageAttributeNames = new List<string> { "eventName" }
            };

            await Task.Run(async () =>
            {
                while (true)
                {
                    try
                    {
                       
                        var result = await _sqs.ReceiveMessageAsync(request);

                        if (result.Messages.Any())
                        {
                            foreach (Message message in result.Messages)
                            {
                                _logger.Information($"Consuming from event {message.MessageAttributes["eventName"].StringValue}");

                                await ProcessEvent(message.Body, message.MessageAttributes["eventName"].StringValue);
                                await _sqs.DeleteMessageAsync(await _amazonSQSConnection.GetCreateQueue(), message.ReceiptHandle);
                            }
                        }
                    }

                    catch (Exception e)
                    {
                     
                        _logger.Error($"Error {e.Message} occured during consuming the event");
                    }
                }
            });

        }

        private async Task ProcessEvent(string message, string eventName)
        {
            _logger.Information($"Processing event: {eventName}");

            if (_subsManager.HasSubscriptionsForEvent(eventName))
            {
                using (var scope = _serviceScopeFactory.CreateScope())
                {
                    var subscriptions = _subsManager.GetHandlersForEvent(eventName);

                    _logger.Information($"Subscriptions number {subscriptions.Count()}");

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
                _logger.Warning($"No subscription for AmazonSQS event: {eventName}");
            }
        }
    }
}
