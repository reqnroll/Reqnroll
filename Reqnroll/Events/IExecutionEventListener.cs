using System.Threading.Tasks;

namespace Reqnroll.Events
{
    public interface IExecutionEventListener
    {
        void OnEvent(IExecutionEvent executionEvent);
    }

    public interface IAsyncExecutionEventListener
    {
        Task OnEventAsync(IExecutionEvent executionEvent);
    }
}
