using ApolloBus.Events;
using ApolloBus.InterfacesAbstraction;
using System;
using System.Collections.Generic;
using static ApolloBus.InMemorySubscriptionsManager;

namespace ApolloBus
{
    public interface ISubscriptionsManager
    {
        bool IsEmpty { get; }
        event EventHandler<string> OnEventRemoved;


        void AddSubscription<T, TH>()
           where T : ApolloEvent
           where TH : IEventHandler<T>;

        void RemoveSubscription<T, TH>()
             where TH : IEventHandler<T>
             where T : ApolloEvent;


        bool HasSubscriptionsForEvent<T>() where T : ApolloEvent;
        bool HasSubscriptionsForEvent(string eventName);
        Type GetEventTypeByName(string eventName);
        void Clear();
        IEnumerable<SubscriptionInfo> GetHandlersForEvent<T>() where T : ApolloEvent;
        IEnumerable<SubscriptionInfo> GetHandlersForEvent(string eventName);
        string GetEventKey<T>();
    }
}
