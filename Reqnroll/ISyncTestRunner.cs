namespace Reqnroll
{
    public interface ISyncTestRunner
    {
        /// <summary>
        /// The ID of the parallel test worker processing the current scenario.
        /// </summary>
        string TestWorkerId { get; }

        FeatureContext FeatureContext { get; }
        ScenarioContext ScenarioContext { get; }

        void OnTestRunStart();
        void OnTestRunEnd();

        void OnFeatureStart(FeatureInfo featureInfo);
        void OnFeatureEnd();

        void OnScenarioInitialize(ScenarioInfo scenarioInfo, RuleInfo ruleInfo);
        void OnScenarioStart();

        void CollectScenarioErrors();
        void OnScenarioEnd();

        void SkipScenario();

        void Given(string text, string multilineTextArg, Table tableArg, string? keyword = null);
        void When(string text, string multilineTextArg, Table tableArg, string? keyword = null);
        void Then(string text, string multilineTextArg, Table tableArg, string? keyword = null);
        void And(string text, string multilineTextArg, Table tableArg, string? keyword = null);
        void But(string text, string multilineTextArg, Table tableArg, string? keyword = null);

        void Pending();
    }
}
