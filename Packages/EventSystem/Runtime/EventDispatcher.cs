using System;
using System.Collections.Generic;

namespace GSDev.EventSystem
{
    public interface IEventSender
    {
        EventDispatcher Dispatcher { get; }
    }
    
    public class EventDispatcher
    {
        public static readonly EventDispatcher Global = new EventDispatcher();
        
        private readonly Dictionary<Type, List<object>> _actions = new Dictionary<Type, List<object>>();
        internal Dictionary<Type, List<object>> Actions => _actions;
        internal static readonly EventPool Pool = new EventPool();

        internal void Register(Type type, object action)
        {
            if (!_actions.TryGetValue(type, out var actionList))
            {
                actionList = new List<object>(4);
                _actions.Add(type, actionList);
            }
            actionList.Add(action);
        }

        internal void Unregister(Type type, object action)
        {
            if (!_actions.TryGetValue(type, out var actionList))
                return;
            actionList.Remove(action);
        }

        internal void DoFire<TEvent>(TEvent arg)
            where TEvent : class, IEvent
        {
            if (!_actions.TryGetValue(typeof(TEvent), out var actionList))
            {
                return;
            }

            foreach (Action<TEvent> action in actionList)
            {
                action.Invoke(arg);
            }
            
            Pool.Recycle(arg);
        }
    }

    public static class EventSender
    {
        public static void DispatchEvent<TEvent>(
            this IEventSender sender, TEvent witness)
            where TEvent : class, IEventBase, new() =>
            sender.Dispatcher.DoFire(EventDispatcher.Pool.Get(witness));

        public static void DispatchEvent<TEvent, TParam1>(
            this IEventSender sender, TEvent witness, TParam1 param1)
            where TEvent : class, IEventBase<TParam1>, new() =>
            sender.Dispatcher.DoFire(EventDispatcher.Pool.Get(witness, param1));

        public static void DispatchEvent<TEvent, TParam1, TParam2>(
            this IEventSender sender, TEvent witness, 
            TParam1 param1, TParam2 param2)
            where TEvent : class, IEventBase<TParam1, TParam2>, new() =>
            sender.Dispatcher.DoFire(EventDispatcher.Pool.Get(witness, param1, param2));

        public static void DispatchEvent<TEvent, TParam1, TParam2, TParam3>(
            this IEventSender sender, TEvent witness, 
            TParam1 param1, TParam2 param2, TParam3 param3)
            where TEvent : class, IEventBase<TParam1, TParam2, TParam3>, new() =>
            sender.Dispatcher.DoFire(EventDispatcher.Pool.Get(witness, param1, param2, param3));

        public static void DispatchEvent<TEvent, TParam1, TParam2, TParam3, TParam4>(
            this IEventSender sender, TEvent witness, 
            TParam1 param1, TParam2 param2, TParam3 param3, TParam4 param4)
            where TEvent : class, IEventBase<TParam1, TParam2, TParam3, TParam4>, new() =>
            sender.Dispatcher.DoFire(EventDispatcher.Pool.Get(witness, param1, param2, param3, param4));

        public static void DispatchEvent<TEvent, TParam1, TParam2, TParam3, TParam4, TParam5>(
            this IEventSender sender, TEvent witness, 
            TParam1 param1, TParam2 param2, TParam3 param3, TParam4 param4, TParam5 param5)
            where TEvent : class, IEventBase<TParam1, TParam2, TParam3, TParam4, TParam5>, new() =>
            sender.Dispatcher.DoFire(EventDispatcher.Pool.Get(witness, param1, param2, param3, param4, param5));

        public static void DispatchEvent<TEvent, TParam1, TParam2, TParam3, TParam4, TParam5, TParam6>(
            this IEventSender sender, TEvent witness,  
            TParam1 param1, TParam2 param2, TParam3 param3,
            TParam4 param4, TParam5 param5, TParam6 param6)
            where TEvent : class, IEventBase<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6>, new() =>
            sender.Dispatcher.DoFire(EventDispatcher.Pool.Get(witness, param1, param2, param3, param4, param5, param6));

        public static void DispatchEvent<TEvent, TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7>(
            this IEventSender sender, TEvent witness, 
            TParam1 param1, TParam2 param2, TParam3 param3,
            TParam4 param4, TParam5 param5, TParam6 param6, TParam7 param7)
            where TEvent : class, IEventBase<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7>, new() =>
            sender.Dispatcher.DoFire(EventDispatcher.Pool.Get(witness, param1, param2, param3, param4, param5, param6, param7));
    }
}