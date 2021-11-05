using ApolloBus.Events;
using ApolloBus.InterfacesAbstraction;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ApolloBus
{
    public partial class InMemorySubscriptionsManager : ISubscriptionsManager
    {
        private readonly Dictionary<string, List<SubscriptionInfo>> _handlers;
        private readonly List<Type> _eventTypes;
        public event EventHandler<string> OnEventRemoved;
        private readonly ILogger _logger;


        public InMemorySubscriptionsManager(ILogger logger)
        {
            _handlers = new Dictionary<string, List<SubscriptionInfo>>();
            _eventTypes = new List<Type>();
            _logger = logger;
        }

        public bool IsEmpty => !_handlers.Keys.Any();

        public void AddSubscription<T, TH>()
            where T : ApolloEvent
            where TH : IEventHandler<T>
        {
            var eventName = GetEventKey<T>();
            _logger.Information($"Add suscription event name {eventName}");

            DoAddSubscription(typeof(TH), eventName, isDynamic: false);

            if (!_eventTypes.Contains(typeof(T)))
            {
                _eventTypes.Add(typeof(T));
            }
        }

        private void DoAddSubscription(Type handlerType, string eventName, bool isDynamic)
        {
            if (!HasSubscriptionsForEvent(eventName))
            {
                _handlers.Add(eventName, new List<SubscriptionInfo>());
            }

            if (_handlers[eventName].Any(s => s.HandlerType == handlerType))
            {
                throw new ArgumentException(
                    $"Handler Type {handlerType.Name} already registered for '{eventName}'", nameof(handlerType));
            }


            _handlers[eventName].Add(SubscriptionInfo.Typed(handlerType));

        }

        public void Clear() => _handlers.Clear();
        

        public string GetEventKey<T>() => typeof(T).Name;


        public Type GetEventTypeByName(string eventName) => _eventTypes.SingleOrDefault(t => t.Name == eventName);

        public IEnumerable<SubscriptionInfo> GetHandlersForEvent(string eventName) => _handlers[eventName];

        public IEnumerable<SubscriptionInfo> GetHandlersForEvent<T>() where T : ApolloEvent
        {
            var key = GetEventKey<T>();
            return GetHandlersForEvent(key);
        }
        public bool HasSubscriptionsForEvent<T>() where T : ApolloEvent
        {
            var key = GetEventKey<T>();
            return HasSubscriptionsForEvent(key);
        }

        public bool HasSubscriptionsForEvent(string eventName) => _handlers.ContainsKey(eventName);


        public void RemoveSubscription<T, TH>()
            where T : ApolloEvent
            where TH : IEventHandler<T>
        {
            var handlerToRemove = FindSubscriptionToRemove<T, TH>();
            var eventName = GetEventKey<T>();

            _logger.Information($"Remove subscription event name {eventName}");

            DoRemoveHandler(eventName, handlerToRemove);
        }

        private SubscriptionInfo FindSubscriptionToRemove<T, TH>()
           where T : ApolloEvent
           where TH : IEventHandler<T>
        {
            var eventName = GetEventKey<T>();
            return DoFindSubscriptionToRemove(eventName, typeof(TH));
        }

        private SubscriptionInfo DoFindSubscriptionToRemove(string eventName, Type handlerType)
        {
            if (!HasSubscriptionsForEvent(eventName))
            {
                return null;
            }

            return _handlers[eventName].SingleOrDefault(s => s.HandlerType == handlerType);

        }
        private void DoRemoveHandler(string eventName, SubscriptionInfo subsToRemove)
        {
            if (subsToRemove != null)
            {
                _handlers[eventName].Remove(subsToRemove);
                if (!_handlers[eventName].Any())
                {
                    _handlers.Remove(eventName);
                    var eventType = _eventTypes.SingleOrDefault(e => e.Name == eventName);
                    if (eventType != null)
                    {
                        _eventTypes.Remove(eventType);
                        _logger.Information($"Remove handler event type {eventType.Name}");
                    }
                    RaiseOnEventRemoved(eventName);
                }

            }
        }
        private void RaiseOnEventRemoved(string eventName)
        {
            var handler = OnEventRemoved;
            _logger.Information($"Raise event to remove {eventName}");
            handler?.Invoke(this, eventName);
        }
    }
}
