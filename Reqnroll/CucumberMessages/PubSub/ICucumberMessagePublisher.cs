using Reqnroll.Events;

namespace Reqnroll.CucumberMessages.PubSub
{
    public interface ICucumberMessagePublisher
    {
        public void HookIntoTestThreadExecutionEventPublisher(ITestThreadExecutionEventPublisher testThreadEventPublisher);
    }
}