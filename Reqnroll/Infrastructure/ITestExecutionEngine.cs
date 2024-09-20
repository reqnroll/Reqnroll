using System.Threading.Tasks;
using Reqnroll.Bindings;

namespace Reqnroll.Infrastructure
{
    public interface ITestExecutionEngine
    {
        FeatureContext FeatureContext { get; }
        ScenarioContext ScenarioContext { get; }
        ITestThreadContext TestThreadContext { get; }

        Task OnTestRunStartAsync();
        Task OnTestRunEndAsync();

        Task OnFeatureStartAsync(FeatureInfo featureInfo);
        Task OnFeatureEndAsync();

        void OnScenarioInitialize(ScenarioInfo scenarioInfo);
        Task OnScenarioStartAsync();
        Task OnAfterLastStepAsync();
        Task OnScenarioEndAsync();

        void OnScenarioSkipped();

        Task StepAsync(StepDefinitionKeyword stepDefinitionKeyword, string keyword, string text, string multilineTextArg, Table tableArg, string pickleStepId);

        void Pending();
    }
}
