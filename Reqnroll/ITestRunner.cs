using System.Threading.Tasks;

namespace Reqnroll
{
    public interface ITestRunner
    {
        /// <summary>
        /// The ID of the parallel test worker processing the current scenario.
        /// </summary>
        string TestWorkerId { get; }
        FeatureContext FeatureContext { get; }
        ScenarioContext ScenarioContext { get; }
        ITestThreadContext TestThreadContext { get; }

        Task OnTestRunStartAsync();
        Task OnTestRunEndAsync();

        Task OnFeatureStartAsync(FeatureInfo featureInfo);
        Task OnFeatureEndAsync();

        void OnScenarioInitialize(ScenarioInfo scenarioInfo, RuleInfo ruleInfo);
        Task OnScenarioStartAsync();

        Task CollectScenarioErrorsAsync();
        Task OnScenarioEndAsync();

        Task SkipScenarioAsync();

        Task GivenAsync(string text, string multilineTextArg, Table tableArg, string keyword = null);
        Task WhenAsync(string text, string multilineTextArg, Table tableArg, string keyword = null);
        Task ThenAsync(string text, string multilineTextArg, Table tableArg, string keyword = null);
        Task AndAsync(string text, string multilineTextArg, Table tableArg, string keyword = null);
        Task ButAsync(string text, string multilineTextArg, Table tableArg, string keyword = null);

        void Pending();
    }
}
