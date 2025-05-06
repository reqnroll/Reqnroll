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

        [Obsolete("ExecutionEvents are migrating to Async. Please migrate to PublishEventAsync", false)]
        public void PublishEvent(IExecutionEvent executionEvent)
        {
            Task.Run(async () => await PublishEventAsync(executionEvent)).Wait();
        }

        public async Task PublishEventAsync(IExecutionEvent executionEvent)
        {
            Task.Run(() =>
            {
                foreach (var listener in _listeners)
                {
                    listener.OnEvent(executionEvent);
                }
            }).Wait();

            foreach (var listener in _asyncListeners)
            {
                await listener.OnEventAsync(executionEvent);
            }

            Task.Run(() =>
            {
                if (_handlersDictionary.TryGetValue(executionEvent.GetType(), out var handlers))
                {
                    foreach (var handler in handlers)
                    {
                        handler.DynamicInvoke(executionEvent);
                    }
                }
            }).Wait();
        }

        [Obsolete("ExecutionEvents are migrating to Async. Please migrate to PublishEventAsync", false)]
        public void AddListener(IExecutionEventListener listener)
        {
            _listeners.Add(listener);
        }

        public void AddAsyncListener(IAsyncExecutionEventListener listener)
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
