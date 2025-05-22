using System;
using System.Threading.Tasks;

namespace Reqnroll.Events
{
    public interface ITestThreadExecutionEventPublisher
    {
        void PublishEvent(IExecutionEvent executionEvent);

        Task PublishEventAsync(IExecutionEvent executionEvent);

        void AddListener(IExecutionEventListener listener);

        void AddAsyncListener(IAsyncExecutionEventListener listener);

        void AddHandler<TEvent>(Action<TEvent> handler) where TEvent: IExecutionEvent;
    }
}
