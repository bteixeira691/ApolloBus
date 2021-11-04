using ApolloBus.Events;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace ApolloBus.InterfacesAbstraction
{
    public interface IApolloBus
    {
        Task Publish(ApolloEvent _event);

        Task PublishDelay(ApolloEvent _event, int seconds);
        Task PublishRecurring(ApolloEvent _event, string CronExpressions);
        Task RemovePublishRecurring();

        Task Subscribe<T, TH>() where T : ApolloEvent where TH : IEventHandler<T>;
    }
}
