using System;
using System.Threading.Tasks;

namespace Reqnroll.Events
{
    public interface ITestThreadExecutionEventPublisher
    {
        [Obsolete("ExecutionEvents are migrating to Async. Please migrate to IAsyncExecutionEventListener", false)]
        void PublishEvent(IExecutionEvent executionEvent);

        Task PublishEventAsync(IExecutionEvent executionEvent);

        [Obsolete("ExecutionEvents are migrating to Async. Please migrate to IAsyncExecutionEventListener", false)]
        void AddListener(IExecutionEventListener listener);

        void AddListener(IAsyncExecutionEventListener listener);

        void AddHandler<TEvent>(Action<TEvent> handler) where TEvent: IExecutionEvent;
    }
}
