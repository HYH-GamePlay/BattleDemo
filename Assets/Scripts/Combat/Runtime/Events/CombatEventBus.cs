using System;
using System.Collections.Generic;

namespace Combat.Runtime.Events
{
    public interface ICombatEvent { }

    public sealed class CombatEventBus
    {
        private readonly Dictionary<Type, List<Delegate>> _handlers = new();

        public void Publish<TEvent>(TEvent combatEvent) where TEvent : struct, ICombatEvent
        {
            if (!_handlers.TryGetValue(typeof(TEvent), out var handlers)) return;

            var snapshot = handlers.ToArray();
            foreach (var handler in snapshot)
                ((Action<TEvent>)handler).Invoke(combatEvent);
        }

        public void Subscribe<TEvent>(Action<TEvent> handler) where TEvent : struct, ICombatEvent
        {
            var eventType = typeof(TEvent);
            if (!_handlers.TryGetValue(eventType, out var handlers))
            {
                handlers = new List<Delegate>();
                _handlers[eventType] = handlers;
            }

            if (!handlers.Contains(handler))
                handlers.Add(handler);
        }

        public void Unsubscribe<TEvent>(Action<TEvent> handler) where TEvent : struct, ICombatEvent
        {
            if (_handlers.TryGetValue(typeof(TEvent), out var handlers))
                handlers.Remove(handler);
        }

        public void Clear() => _handlers.Clear();
    }
}
