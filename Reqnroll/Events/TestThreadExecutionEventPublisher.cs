using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Reqnroll.Events
{
    public class TestThreadExecutionEventPublisher : ITestThreadExecutionEventPublisher
    {
        private readonly List<IExecutionEventListener> _listeners = new();
        private readonly List<IAsyncExecutionEventListener> _asyncListeners = new();
        private readonly Dictionary<Type, List<Delegate>> _handlersDictionary = new();

        /// <summary>
        /// Publishes the specified execution event to the appropriate handlers or subscribers.
        /// </summary>
        /// <remarks>This method blocks the calling thread until the event is published asynchronously.</remarks>
        /// <param name="executionEvent">The execution event to be published. Cannot be <see langword="null"/>.</param>
        [Obsolete("ExecutionEvents are migrating to Async. Please migrate to PublishEventAsync", false)]
        public void PublishEvent(IExecutionEvent executionEvent)
        {
            Task.Run(async () => await PublishEventAsync(executionEvent)).Wait();
        }

        public async Task PublishEventAsync(IExecutionEvent executionEvent)
        {
            foreach (var listener in _listeners)
            {
                listener.OnEvent(executionEvent);
            }

            foreach (var listener in _asyncListeners)
            {
                await listener.OnEventAsync(executionEvent);
            }

            if (_handlersDictionary.TryGetValue(executionEvent.GetType(), out var handlers))
            {
                foreach (var handler in handlers)
                {
                    handler.DynamicInvoke(executionEvent);
                }
            }
        }

        [Obsolete("ExecutionEvents are migrating to Async. Please migrate to PublishEventAsync", false)]
        public void AddListener(IExecutionEventListener listener)
        {
            _listeners.Add(listener);
        }

        public void AddListener(IAsyncExecutionEventListener listener)
        {
            _asyncListeners.Add(listener);
        }

        public void AddHandler<TEvent>(Action<TEvent> handler) where TEvent : IExecutionEvent
        {
            if (!_handlersDictionary.TryGetValue(typeof(TEvent), out var handlers))
            {
                handlers = new List<Delegate> { handler };
                _handlersDictionary.Add(typeof(TEvent), handlers);
            }
            else
            {
                handlers.Add(handler);
            }
        }
    }
}
