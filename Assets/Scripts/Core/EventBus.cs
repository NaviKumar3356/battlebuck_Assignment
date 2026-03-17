// EventBus.cs
// A lightweight, generic, static event bus.
//
// Architecture decision: a static event bus decouples systems completely —
// the kill simulator does not need a reference to the score system or the UI.
// Each system subscribes to the events it cares about and reacts independently.
// This makes it trivial to add or remove systems without changing existing code.
//
// Generic handlers are stored in a per-type static dictionary so that
// subscription/dispatch is O(1) per handler and causes zero boxing for structs
// when used with the non-boxed overloads.

using System;
using System.Collections.Generic;
using UnityEngine;

namespace DeathMatch.Core
{
    public static class EventBus
    {
        // Stores one delegate list per event type.
        private static readonly Dictionary<Type, Delegate> _handlers = new Dictionary<Type, Delegate>(16);

        /// <summary>Subscribe to an event of type <typeparamref name="T"/>.</summary>
        public static void Subscribe<T>(Action<T> handler) where T : struct
        {
            Type key = typeof(T);
            if (_handlers.TryGetValue(key, out Delegate existing))
            {
                _handlers[key] = Delegate.Combine(existing, handler);
            }
            else
            {
                _handlers[key] = handler;
            }
        }

        /// <summary>Unsubscribe from an event. Always call this in OnDestroy to prevent memory leaks.</summary>
        public static void Unsubscribe<T>(Action<T> handler) where T : struct
        {
            Type key = typeof(T);
            if (_handlers.TryGetValue(key, out Delegate existing))
            {
                Delegate updated = Delegate.Remove(existing, handler);
                if (updated == null)
                    _handlers.Remove(key);
                else
                    _handlers[key] = updated;
            }
        }

        /// <summary>Fire an event. All subscribers are notified immediately (synchronous).</summary>
        public static void Raise<T>(T eventData) where T : struct
        {
            Type key = typeof(T);
            if (_handlers.TryGetValue(key, out Delegate handler))
            {
                try
                {
                    ((Action<T>)handler).Invoke(eventData);
                }
                catch (Exception e)
                {
                    Debug.LogError($"[EventBus] Exception in handler for {key.Name}: {e}");
                }
            }
        }

        /// <summary>Clear all subscriptions. Call between scenes or on application quit.</summary>
        public static void Clear() => _handlers.Clear();
    }
}