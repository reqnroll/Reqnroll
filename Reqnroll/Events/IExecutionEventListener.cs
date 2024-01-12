namespace Reqnroll.Events
{
    public interface IExecutionEventListener
    {
        void OnEvent(IExecutionEvent executionEvent);
    }
}
