using Reqnroll.Events;

namespace Reqnroll.CucumberMessages
{
    public interface ICucumberMessagePublisher
    {
        public void HookIntoTestThreadExecutionEventPublisher(ITestThreadExecutionEventPublisher testThreadEventPublisher);
    }
}