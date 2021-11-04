using Amazon.SQS;
using Amazon.SQS.Model;
using ApolloBus.AmazonSQS.Model;
using ApolloBus.Polly;
using Serilog;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace ApolloBus.AmazonSQS
{
    public class AmazonSQSConnection
    {
        private readonly ComplementaryConfig _complementaryConfigAmazonSQS;
        private readonly IAmazonSQS _amazonSQS;
        private readonly ILogger _logger;
        private readonly IPollyPolicy _pollyPolicy;

        public AmazonSQSConnection(ComplementaryConfig complementaryConfigAmazonSQS, IAmazonSQS amazonSQS, ILogger logger, IPollyPolicy pollyPolicy)
        {
            _complementaryConfigAmazonSQS = complementaryConfigAmazonSQS ?? throw new ArgumentNullException(nameof(complementaryConfigAmazonSQS));
            _amazonSQS = amazonSQS ?? throw new ArgumentNullException(nameof(amazonSQS));
            _pollyPolicy = pollyPolicy ?? throw new ArgumentNullException(nameof(pollyPolicy));

            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }


        public async Task<string> GetCreateQueue()
        {
            string sqsQueue="";
            var policy = _pollyPolicy.ApolloRetryPolicyConnect();

            await policy.ExecuteAsync(async () =>
            {
                sqsQueue = await CheckQueue();

                if (string.IsNullOrEmpty(sqsQueue))
                    sqsQueue = (await _amazonSQS.CreateQueueAsync(_complementaryConfigAmazonSQS.QueueName)).QueueUrl;


                _logger.Information($"Queue url {sqsQueue}");
                return sqsQueue;
            });

            return sqsQueue;
        }

        private async Task<string> CheckQueue()
        {
            var policy = _pollyPolicy.ApolloRetryPolicyConnect();
            await policy.ExecuteAsync(async () =>
            {
                var sqsQueue = await _amazonSQS.GetQueueUrlAsync(_complementaryConfigAmazonSQS.QueueName);
                return sqsQueue.QueueUrl;
            });

            return string.Empty;
        }
    }
}
