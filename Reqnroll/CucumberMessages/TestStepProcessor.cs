using Io.Cucumber.Messages.Types;
using Reqnroll.Assist;
using Reqnroll.Bindings;
using Reqnroll.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace Reqnroll.CucumberMessages
{
    public class StepArgument
    {
        public string Value;
        public string Type;
    }

    public class TestStepProcessor : StepProcessorBase
    {
        private StepStartedEvent stepStartedEvent;

        public TestStepProcessor(TestCaseCucumberMessageTracker parentTracker) : base(parentTracker)
        {
        }

        public string PickleStepID { get; set; }
        public bool Bound { get; set; }
        public string CanonicalizedStepPattern { get; set; }
        public string StepDefinitionId { get; private set; }
        public IEnumerable<string> AmbiguousStepDefinitions { get; set; }
        public bool Ambiguous {  get { return AmbiguousStepDefinitions != null && AmbiguousStepDefinitions.Count() > 0;} }
        public IStepDefinitionBinding StepDefinition { get; set; }

        public List<StepArgument> StepArguments { get; set; }

        internal void ProcessEvent(StepStartedEvent stepStartedEvent)
        {
            this.stepStartedEvent = stepStartedEvent;
            TestStepID = ParentTestCase.IDGenerator.GetNewId();
            PickleStepID = stepStartedEvent.StepContext.StepInfo.PickleStepId;
        }

        private string FindStepDefIDByStepPattern(string canonicalizedStepPattern)
        {
            return ParentTestCase.StepDefinitionsByPattern[canonicalizedStepPattern];
        }

        internal void ProcessEvent(StepFinishedEvent stepFinishedEvent)
        {
            var bindingMatch = stepFinishedEvent.StepContext?.StepInfo?.BindingMatch;
            Bound = !(bindingMatch == null || bindingMatch == BindingMatch.NonMatching);

            StepDefinition = Bound ? bindingMatch.StepBinding : null;
            CanonicalizedStepPattern = Bound ? CucumberMessageFactory.CanonicalizeStepDefinitionPattern(StepDefinition) : "";
            StepDefinitionId = Bound ? FindStepDefIDByStepPattern(CanonicalizedStepPattern) : null;

            Duration = stepFinishedEvent.Timestamp - stepStartedEvent.Timestamp;
            Status = stepFinishedEvent.StepContext.Status;

            if (Status == ScenarioExecutionStatus.TestError && stepFinishedEvent.ScenarioContext.TestError != null)
            {
                Exception = stepFinishedEvent.ScenarioContext.TestError;
                if (Exception is AmbiguousBindingException)
                {
                    AmbiguousStepDefinitions = new List<string>(((AmbiguousBindingException)Exception).Matches.Select(m => 
                                                    FindStepDefIDByStepPattern(CucumberMessageFactory.CanonicalizeStepDefinitionPattern(m.StepBinding))));
                }
            }

            var IsInputDataTableOrDocString = stepFinishedEvent.StepContext.StepInfo.Table != null || stepFinishedEvent.StepContext.StepInfo.MultilineText != null;
            var argumentValues = Bound ? stepFinishedEvent.StepContext.StepInfo.BindingMatch.Arguments.Select(arg => arg.ToString()).ToList() : new List<string>();
            var argumentTypes = Bound ? stepFinishedEvent.StepContext.StepInfo.BindingMatch.StepBinding.Method.Parameters.Select(p => p.Type.Name).ToList() : new List<string>();
            StepArguments = Bound && !IsInputDataTableOrDocString ?
                argumentValues.Zip(argumentTypes, (x, y) => new StepArgument { Value = x, Type = y }).ToList<StepArgument>()
                : Enumerable.Empty<StepArgument>().ToList<StepArgument>();

        }
    }
}