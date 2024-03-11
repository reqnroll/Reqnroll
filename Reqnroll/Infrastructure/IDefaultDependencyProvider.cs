using Reqnroll.BoDi;

namespace Reqnroll.Infrastructure
{
    public interface IDefaultDependencyProvider
    {
        void RegisterGlobalContainerDefaults(ObjectContainer container);
        void RegisterTestThreadContainerDefaults(ObjectContainer testThreadContainer);
        void RegisterScenarioContainerDefaults(ObjectContainer scenarioContainer);
    }
}