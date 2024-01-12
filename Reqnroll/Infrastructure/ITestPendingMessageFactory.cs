namespace Reqnroll.Infrastructure
{
    public interface ITestPendingMessageFactory
    {
        string BuildFromScenarioContext(ScenarioContext scenarioContext);
    }
}
