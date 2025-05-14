using Io.Cucumber.Messages.Types;
using Reqnroll.Bindings;
using Reqnroll.CucumberMessages.PayloadProcessing.Cucumber;
using Reqnroll.Events;
using System.Collections.Generic;
using System.Linq;

namespace Reqnroll.CucumberMessages.ExecutionTracking
{
    public class TestStepArgument
    {
        public string Value;
        public int? StartOffset;
        public string Type;
    }


    /// <summary>
    /// Data class that captures information about a TestStep that is being executed for the first time.
    /// One of these is created per step, regardless of how many times the Test Case is retried.
    /// </summary>
    public class TestStepDefinition
    {
        // The Id of the Step within the TestCase
        internal string TestStepId { get; }

        // The Id of the PickleStep
        internal string PickleStepID { get; }
        internal TestCaseDefinition ParentTestCaseDefinition { get; }

        internal ICucumberMessageFactory _messageFactory;

        // The Step Definition(s) that match this step of the Test Case. None for no match, 1 for a successful match, 2 or more for Ambiguous match.
        internal List<string> StepDefinitionIds { get; private set; }
        // Indicates whether the step was successfully bound to a Step Definition.
        internal bool Bound { get; private set; }

        // The method name and signature of the bound method
        private string CanonicalizedStepPattern { get; set; }

        // List of method signatures that cause an ambiguous situation to arise
        private IEnumerable<string> AmbiguousStepDefinitions { get; set; }
        internal bool Ambiguous { get { return AmbiguousStepDefinitions != null && AmbiguousStepDefinitions.Count() > 0; } }
        private IStepDefinitionBinding StepDefinitionBinding;

        internal List<TestStepArgument> StepArguments { get; private set; } = new();


        internal TestStepDefinition(string testStepDefinitionId, string pickleStepId, TestCaseDefinition parentTestCaseDefinition, ICucumberMessageFactory messageFactory)
        {
            TestStepId = testStepDefinitionId;
            PickleStepID = pickleStepId;
            ParentTestCaseDefinition = parentTestCaseDefinition;
            _messageFactory = messageFactory;
        }

        // Once the StepFinished event fires, we can finally capture which step binding was used and the arguments sent as parameters to the binding method
        internal void PopulateStepDefinitionFromExecutionResult(StepFinishedEvent stepFinishedEvent)
        {
            var bindingMatch = stepFinishedEvent.StepContext?.StepInfo?.BindingMatch;
            Bound = !(bindingMatch == null || bindingMatch == BindingMatch.NonMatching);

            StepDefinitionBinding = Bound ? bindingMatch.StepBinding : null;
            CanonicalizedStepPattern = Bound ? _messageFactory.CanonicalizeStepDefinitionPattern(StepDefinitionBinding) : "";
            var StepDefinitionId = Bound ? ParentTestCaseDefinition.FindStepDefIDByStepPattern(CanonicalizedStepPattern) : null;

            var Status = stepFinishedEvent.StepContext.Status;

            if (Status == ScenarioExecutionStatus.TestError && stepFinishedEvent.ScenarioContext.TestError != null)
            {
                var Exception = stepFinishedEvent.ScenarioContext.TestError;
                if (Exception is AmbiguousBindingException)
                {
                    AmbiguousStepDefinitions = new List<string>(((AmbiguousBindingException)Exception).Matches.Select(m =>
                                                    ParentTestCaseDefinition.FindStepDefIDByStepPattern(_messageFactory.CanonicalizeStepDefinitionPattern(m.StepBinding))));
                }
            }

            StepDefinitionIds = Ambiguous ? AmbiguousStepDefinitions.ToList() : StepDefinitionId != null ? [StepDefinitionId] : [];

            var IsInputDataTableOrDocString = stepFinishedEvent.StepContext.StepInfo.Table != null || stepFinishedEvent.StepContext.StepInfo.MultilineText != null;
            var argumentValues = Bound ? stepFinishedEvent.StepContext.StepInfo.BindingMatch.Arguments.Select(arg => arg.Value.ToString()).ToList() : new List<string>();
            var argumentStartOffsets = Bound ? stepFinishedEvent.StepContext.StepInfo.BindingMatch.Arguments.Select(arg => arg.StartOffset).ToList() : new List<int?>();
            var argumentTypes = Bound ? stepFinishedEvent.StepContext.StepInfo.BindingMatch.StepBinding.Method.Parameters.Select(p => p.Type.Name).ToList() : new List<string>();

            if (Bound && !IsInputDataTableOrDocString)
            {
                for (int i = 0; i < argumentValues.Count; i++)
                {
                    StepArguments.Add(new TestStepArgument { Value = argumentValues[i], StartOffset = argumentStartOffsets[i], Type = argumentTypes[i] });
                }
            }
        }
    }
}