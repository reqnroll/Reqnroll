using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace Reqnroll.Events
{
    public class TestThreadExecutionEventPublisher : ITestThreadExecutionEventPublisher
    {
        private readonly List<IExecutionEventListener> _listeners = new();
        private readonly List<IAsyncExecutionEventListener> _asyncListeners = new();
        private readonly Dictionary<Type, List<Delegate>> _handlersDictionary = new();

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
