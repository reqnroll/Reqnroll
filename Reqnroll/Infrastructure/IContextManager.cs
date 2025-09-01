using Reqnroll.Bindings;
using System.Threading.Tasks;

namespace Reqnroll.Infrastructure
{
    public interface IContextManager
    {
        TestThreadContext TestThreadContext { get; }
        FeatureContext FeatureContext { get; }
        ScenarioContext ScenarioContext { get; }
        ScenarioStepContext StepContext { get; }
        StepDefinitionType? CurrentTopLevelStepDefinitionType { get; }

        Task InitializeFeatureContextAsync(FeatureInfo featureInfo);
        ValueTask CleanupFeatureContextAsync();

        Task InitializeScenarioContextAsync(ScenarioInfo scenarioInfo, RuleInfo ruleInfo);
        ValueTask CleanupScenarioContextAsync();

        void InitializeStepContext(StepInfo stepInfo);
        void CleanupStepContext();
    }
}
