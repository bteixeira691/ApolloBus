using Polly.Retry;
using System;

namespace ApolloBus.Polly
{
    public interface IPollyPolicy
    {
        AsyncRetryPolicy ApolloRetryPolicyEvent(Guid _eventId);
        AsyncRetryPolicy ApolloRetryPolicyConnect();
    }
}