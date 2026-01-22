using MTGOnline.Interfaces;

namespace MTGOnline.Events
{
    /// <summary>
    /// Manages game events and subscriptions.
    /// </summary>
    public class EventManager : IEventManager
    {
        private readonly Dictionary<Type, List<Delegate>> _handlers = new();
        private readonly List<IGameEvent> _eventHistory = new();

        public IReadOnlyList<IGameEvent> EventHistory => _eventHistory.AsReadOnly();

        public void RaiseEvent(IGameEvent gameEvent)
        {
            _eventHistory.Add(gameEvent);

            var eventType = gameEvent.GetType();

            // Get handlers for the specific event type
            if (_handlers.TryGetValue(eventType, out var specificHandlers))
            {
                foreach (var handler in specificHandlers.ToList())
                {
                    try
                    {
                        handler.DynamicInvoke(gameEvent);
                    }
                    catch (Exception)
                    {
                        // Log error in real implementation
                    }
                }
            }

            // Get handlers for interfaces the event implements
            foreach (var interfaceType in eventType.GetInterfaces())
            {
                if (_handlers.TryGetValue(interfaceType, out var interfaceHandlers))
                {
                    foreach (var handler in interfaceHandlers.ToList())
                    {
                        try
                        {
                            handler.DynamicInvoke(gameEvent);
                        }
                        catch (Exception)
                        {
                            // Log error in real implementation
                        }
                    }
                }
            }
        }

        public void Subscribe<T>(Action<T> handler) where T : IGameEvent
        {
            var eventType = typeof(T);

            if (!_handlers.ContainsKey(eventType))
            {
                _handlers[eventType] = new List<Delegate>();
            }

            _handlers[eventType].Add(handler);
        }

        public void Unsubscribe<T>(Action<T> handler) where T : IGameEvent
        {
            var eventType = typeof(T);

            if (_handlers.TryGetValue(eventType, out var handlers))
            {
                handlers.Remove(handler);
            }
        }

        /// <summary>
        /// Clears all event subscriptions.
        /// </summary>
        public void ClearSubscriptions()
        {
            _handlers.Clear();
        }

        /// <summary>
        /// Clears event history.
        /// </summary>
        public void ClearHistory()
        {
            _eventHistory.Clear();
        }

        /// <summary>
        /// Gets recent events of a specific type.
        /// </summary>
        public IReadOnlyList<T> GetRecentEvents<T>(int count = 10) where T : IGameEvent
        {
            return _eventHistory
                .OfType<T>()
                .TakeLast(count)
                .ToList()
                .AsReadOnly();
        }
    }
}
