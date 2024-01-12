namespace Reqnroll.Infrastructure
{
    public interface ISkippedStepHandler
    {
        void Handle(ScenarioContext scenarioContext);
    }
}