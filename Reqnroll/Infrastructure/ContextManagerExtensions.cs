using Reqnroll.Bindings;

namespace Reqnroll.Infrastructure
{
    internal static class ContextManagerExtensions
    {
        public static StepContext GetStepContext(this IContextManager contextManager)
        {
            return new StepContext(
                contextManager.FeatureContext?.FeatureInfo,
                contextManager.ScenarioContext?.ScenarioInfo);
        }
    }
}