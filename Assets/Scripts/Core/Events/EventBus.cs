using System;
using System.Collections.Generic;
using UnityEngine;

namespace Core.Events
{
    public class EventBus : IEventBus
    {
        private readonly Dictionary<Type, List<Delegate>> _eventHandlers = new();

        public void Publish<T>(T eventData) where T : struct
        {
            var eventType = typeof(T);
            if (!_eventHandlers.ContainsKey(eventType))
                return;

            var handlers = _eventHandlers[eventType];
            for (int i = handlers.Count - 1; i >= 0; i--)
            {
                try
                {
                    ((Action<T>)handlers[i])?.Invoke(eventData);
                }
                catch (Exception e)
                {
                    Debug.LogError($"Error executing event handler for {eventType.Name}: {e.Message}");
                }
            }
        }

        public void Subscribe<T>(Action<T> handler) where T : struct
        {
            Type eventType = typeof(T);
            if (!_eventHandlers.ContainsKey(eventType))
                _eventHandlers[eventType] = new List<Delegate>();

            if (!_eventHandlers[eventType].Contains(handler))
                _eventHandlers[eventType].Add(handler);
        }

        public void Unsubscribe<T>(Action<T> handler) where T : struct
        {
            Type eventType = typeof(T);
            if (_eventHandlers.ContainsKey(eventType))
                _eventHandlers[eventType].Remove(handler);
        }

        public void Clear()
        {
            _eventHandlers.Clear();
        }
    }
}
