using Reqnroll.Bindings;

namespace Reqnroll.Infrastructure
{
    public interface ITestUndefinedMessageFactory
    {
        string BuildFromContext(ScenarioContext scenarioContext, FeatureContext featureContext);
        string BuildStepMessageFromContext(StepInstance step, FeatureContext featureContext);
    }
}
