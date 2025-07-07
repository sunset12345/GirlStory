using System;
using System.Collections.Generic;

namespace GSDev.EventSystem
{
    public class EventSubscriber : IDisposable
    {
        private readonly List<Tuple<EventDispatcher, Type, object>> _subscriptions = new List<Tuple<EventDispatcher, Type, object>>();

        private static EventDispatcher GetEventDispatcher(IEventSender eventSender)
        {
            return eventSender == null ? 
                EventDispatcher.Global : 
                eventSender.Dispatcher;
        }

        public void Subscribe(Type eventType, Delegate handler, IEventSender eventSender = null)
        {
            if (eventType.GetInterface(nameof(IEvent)) == null)
                return;
            if (handler.GetType().GetGenericTypeDefinition() != typeof(Action<>))
                return;
            var dispatcher = GetEventDispatcher(eventSender);
            _subscriptions.Add(new Tuple<EventDispatcher, Type, object>(dispatcher, eventType, handler));
            dispatcher.Register(eventType, handler);
        }

        public void Unsubscribe(Type eventType, Delegate handler, IEventSender eventSender = null)
        {
            var dispatcher = GetEventDispatcher(eventSender);
            dispatcher.Unregister(eventType, handler);
            for (var i = 0; i < _subscriptions.Count; ++i)
            {
                var tuple = _subscriptions[i];
                if (tuple.Item1 == dispatcher &&
                    tuple.Item2 == eventType &&
                    tuple.Item3 == handler)
                {
                    _subscriptions.RemoveAt(i);
                    break;
                }
            }
        }

        public void Subscribe<T>(Action<T> handler, IEventSender eventSender = null) where T : class, IEvent, new()
        {
            var dispatcher = GetEventDispatcher(eventSender);
            _subscriptions.Add(new Tuple<EventDispatcher, Type, object>(dispatcher, typeof(T), handler));
            dispatcher.Register(typeof(T), handler);
        }

        public void Unsubscribe<T>(Action<T> handler, IEventSender eventSender = null) where T : class, IEvent, new()
        {
            var eventType = typeof(T);
            var dispatcher = GetEventDispatcher(eventSender);
            dispatcher.Unregister(eventType, handler);
            for (var i = 0; i < _subscriptions.Count; ++i)
            {
                var tuple = _subscriptions[i];
                if (tuple.Item1 == dispatcher &&
                    tuple.Item2 == eventType &&
                    (Action<T>)tuple.Item3 == handler)
                {
                    _subscriptions.RemoveAt(i);
                    break;
                }
            }
        }

        public void Unsubscribe<T>(IEventSender eventSender = null) where T : class, IEvent, new()
        {
            var eventType = typeof(T);
            var dispatcher = GetEventDispatcher(eventSender);
            for (var i = 0; i < _subscriptions.Count;)
            {
                var tuple = _subscriptions[i];
                if (tuple.Item1 == dispatcher &&
                    tuple.Item2 == eventType)
                {
                    dispatcher.Unregister(eventType, tuple.Item3);
                    _subscriptions.RemoveAt(i);
                }
                else
                    ++i;
            }
        }
        
        public void Dispose()
        {
            foreach (var (dispatcher, type, action) in _subscriptions)
            {
                dispatcher.Unregister(type, action);
            }
            _subscriptions.Clear();
        }
    }
}