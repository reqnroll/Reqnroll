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

    public class ScenarioStepProcessor : StepProcessorBase
    {
        private StepStartedEvent stepStartedEvent;

        public ScenarioStepProcessor(ScenarioEventProcessor parentScenarioState) : base(parentScenarioState)
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

        internal IEnumerable<Envelope> ProcessEvent(StepStartedEvent stepStartedEvent)
        {
            this.stepStartedEvent = stepStartedEvent;
            TestStepID = parentScenario.IdGenerator.GetNewId();
            return Enumerable.Empty<Envelope>();
        }

        private string FindStepDefIDByStepPattern(string canonicalizedStepPattern)
        {
            return parentScenario.FeatureState.StepDefinitionsByPattern[canonicalizedStepPattern];
        }

        private string FindPickleStepIDByStepText(string stepText)
        {
            return parentScenario.FeatureState.PicklesByScenarioName[parentScenario.Name].Steps.Where(st => st.Text == stepText).First().Id;
        }

        internal IEnumerable<Envelope> ProcessEvent(StepFinishedEvent stepFinishedEvent)
        {
            var bindingMatch = stepFinishedEvent.StepContext?.StepInfo?.BindingMatch;
            Bound = !(bindingMatch == null || bindingMatch == BindingMatch.NonMatching);

            StepDefinition = Bound ? bindingMatch.StepBinding : null;
            CanonicalizedStepPattern = Bound ? CucumberMessageFactory.CanonicalizeStepDefinitionPattern(StepDefinition) : "";
            StepDefinitionId = Bound ? FindStepDefIDByStepPattern(CanonicalizedStepPattern) : null;

            PickleStepID = FindPickleStepIDByStepText(stepFinishedEvent.StepContext.StepInfo.Text);

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

            var argumentValues = Bound ? stepFinishedEvent.StepContext.StepInfo.BindingMatch.Arguments.Select(arg => arg.ToString()).ToList() : new List<string>();
            var argumentTypes = Bound ? stepFinishedEvent.StepContext.StepInfo.BindingMatch.StepBinding.Method.Parameters.Select(p => p.Type.Name).ToList() : new List<string>();
            StepArguments = Bound ?
                argumentValues.Zip(argumentTypes, (x, y) => new StepArgument { Value = x, Type = y }).ToList<StepArgument>()
                : Enumerable.Empty<StepArgument>().ToList<StepArgument>();

            return Enumerable.Empty<Envelope>();
        }
    }
}