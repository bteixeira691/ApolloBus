using ApolloBus.RabbitMQ.Model.Interfaces;
using ApolloBus.Validation;
using Confluent.Kafka;
using Polly;
using Polly.Retry;
using Serilog;
using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;

namespace ApolloBus.Polly
{
    public class PollyPolicy : IPollyPolicy
    {
        private readonly ILogger _logger;
        private readonly IComplementaryConfig _complementaryConfig;

        public PollyPolicy(ILogger logger, IComplementaryConfig complementaryConfig)
        {
            _logger = logger;
            _complementaryConfig = complementaryConfig;
        }



        public AsyncRetryPolicy ApolloRetryPolicyEvent(Guid _eventId)
        {
            return RetryPolicy.Handle<Exception>()
              .Or<SocketException>()
              .Or<KafkaException>()
              .WaitAndRetryAsync(_complementaryConfig.Retry, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)), (ex, time) =>
              {
                  _logger.Error(ex, "Could not publish event: {EventId} after {Timeout}s ({ExceptionMessage})", _eventId, $"{time.TotalSeconds:n1}", ex.Message);

              });
        }

        public AsyncRetryPolicy ApolloRetryPolicyConnect()
        {
            return RetryPolicy.Handle<SocketException>()
                    .Or<Exception>()
                    .WaitAndRetryAsync(_complementaryConfig.Retry, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)), (ex, time) =>
                    {
                        _logger.Error(ex, "Could not connect after {TimeOut}s ({ExceptionMessage})", $"{time.TotalSeconds:n1}", ex.Message);
                    }
                );
        }
    }
}
