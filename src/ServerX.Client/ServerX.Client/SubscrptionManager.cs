﻿using System;
using System.Collections.Generic;
using System.Linq;

namespace ServerX.Client
{
    public interface IEventHandler<in T> : IEventHandler
    {
        void Handle(T @event);

    }
    public interface IEventHandler
    {
    }
    internal class SubscrptionManager
    {
        Dictionary<Type, List<SubInfo>> _handlers;
        readonly Dictionary<string, Type> _eventTypes;
        Func<string, string> _getLocalNameHandler;
        public SubscrptionManager()
        {
            _handlers = new Dictionary<Type, List<SubInfo>>();
            _eventTypes = new Dictionary<string, Type>();
        }
        internal IEnumerable<SubInfo> GetHandlers(string eventName, out Type eventType)
        {
            eventType = GetEventTypeByName(eventName);
            if (eventType == null) return new List<SubInfo>();
            var flag = _handlers.TryGetValue(eventType, out List<SubInfo> result);
            if (!flag) result = new List<SubInfo>();
            return result;
        }
        Type GetEventTypeByName(string eventName)
        {
            var name = _getLocalNameHandler?.Invoke(eventName);
            if (string.IsNullOrEmpty(name)) name = eventName;
            if (!HasSubscriptionsForEvent(name)) return null;
            return _eventTypes[name];
        }
        internal void RegisterServerEventNameToLocalEventName(Func<string, string> func)
        {
            _getLocalNameHandler = func;
        }
        internal void Subscrption<T, THandler>(THandler instance = default(THandler)) where THandler : IEventHandler<T>
        {
            var enentType = typeof(T);
            var name = enentType.FullName;
            var f = _eventTypes.TryAdd(name, enentType);
            if (f) _handlers.Add(enentType, new List<SubInfo>());
            var handlerType = typeof(THandler);
            if (_handlers[enentType].Any(s => s.HandlerType == handlerType))
            {
                throw new ArgumentException($"Handler Type {handlerType.GetType().Name} already registered for '{name}'", nameof(handlerType));
            }
            _handlers[enentType].Add(SubInfo.Create(handlerType, instance));
        }
        private bool HasSubscriptionsForEvent(string eventName)
        {
            return _eventTypes.ContainsKey(eventName);
        }
    }
}
