using Io.Cucumber.Messages.Types;
using Reqnroll.Bindings;
using Reqnroll.CucumberMessages.PayloadProcessing.Cucumber;
using Reqnroll.Events;
using System.Collections.Generic;
using System.Linq;

namespace Reqnroll.CucumberMessages.ExecutionTracking
{
    public class TestStepDefinition
    {
        // The Id of the Step within the TestCase
        public string TestStepId { get; private set; }


        // The Id of the PickleStep
        public string PickleStepID { get; set; }
        public TestCaseDefinition ParentTestCaseDefinition { get; }

        // The Step Definition(s) that match this step of the Test Case. None for no match, 1 for a successful match, 2 or more for Ambiguous match.
        public List<string> StepDefinitionIds { get; private set; }
        public bool Bound { get; set; }
        public string CanonicalizedStepPattern { get; set; }
        private IEnumerable<string> AmbiguousStepDefinitions { get; set; }
        public bool Ambiguous { get { return AmbiguousStepDefinitions != null && AmbiguousStepDefinitions.Count() > 0; } }
        private IStepDefinitionBinding StepDefinitionBinding;

        public List<StepArgument> StepArguments { get; set; }


        public TestStepDefinition(string testStepDefinitionId, string pickleStepId, TestCaseDefinition parentTestCaseDefinition)
        {
            TestStepId = testStepDefinitionId;
            PickleStepID = pickleStepId;
            ParentTestCaseDefinition = parentTestCaseDefinition;
        }

        public void PopulateStepDefinitionFromExecutionResult(StepFinishedEvent stepFinishedEvent)
        {
            var bindingMatch = stepFinishedEvent.StepContext?.StepInfo?.BindingMatch;
            Bound = !(bindingMatch == null || bindingMatch == BindingMatch.NonMatching);

            StepDefinitionBinding = Bound ? bindingMatch.StepBinding : null;
            CanonicalizedStepPattern = Bound ? CucumberMessageFactory.CanonicalizeStepDefinitionPattern(StepDefinitionBinding) : "";
            var StepDefinitionId = Bound ? ParentTestCaseDefinition.FindStepDefIDByStepPattern(CanonicalizedStepPattern) : null;

            var Status = stepFinishedEvent.StepContext.Status;

            if (Status == ScenarioExecutionStatus.TestError && stepFinishedEvent.ScenarioContext.TestError != null)
            {
                var Exception = stepFinishedEvent.ScenarioContext.TestError;
                if (Exception is AmbiguousBindingException)
                {
                    AmbiguousStepDefinitions = new List<string>(((AmbiguousBindingException)Exception).Matches.Select(m =>
                                                    ParentTestCaseDefinition.FindStepDefIDByStepPattern(CucumberMessageFactory.CanonicalizeStepDefinitionPattern(m.StepBinding))));
                }
            }

            StepDefinitionIds = Ambiguous ? AmbiguousStepDefinitions.ToList() : StepDefinitionId != null ? [StepDefinitionId] : [];

            var IsInputDataTableOrDocString = stepFinishedEvent.StepContext.StepInfo.Table != null || stepFinishedEvent.StepContext.StepInfo.MultilineText != null;
            var argumentValues = Bound ? stepFinishedEvent.StepContext.StepInfo.BindingMatch.Arguments.Select(arg => arg.ToString()).ToList() : new List<string>();
            var argumentTypes = Bound ? stepFinishedEvent.StepContext.StepInfo.BindingMatch.StepBinding.Method.Parameters.Select(p => p.Type.Name).ToList() : new List<string>();
            StepArguments = Bound && !IsInputDataTableOrDocString ?
                argumentValues.Zip(argumentTypes, (x, y) => new StepArgument { Value = x, Type = y }).ToList()
                : Enumerable.Empty<StepArgument>().ToList();

        }
    }
}