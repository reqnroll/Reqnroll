using Reqnroll.Events;

namespace Reqnroll.CucumberMesssages
{
    public interface ICucumberMessagePublisher
    {
        public void HookIntoTestThreadExecutionEventPublisher(ITestThreadExecutionEventPublisher testThreadEventPublisher);
    }
}