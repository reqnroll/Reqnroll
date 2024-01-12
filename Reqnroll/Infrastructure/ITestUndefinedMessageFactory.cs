namespace Reqnroll.Infrastructure
{
    public interface ITestUndefinedMessageFactory
    {
        string BuildFromContext(ScenarioContext scenarioContext, FeatureContext featureContext);
    }
}
