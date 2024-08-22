using Io.Cucumber.Messages.Types;
using Reqnroll.Assist;
using Reqnroll.Bindings;
using Reqnroll.Events;
using System.Collections.Generic;
using System.Linq;

namespace Reqnroll.CucumberMesssages
{
    public class StepArgument
    {
        public string Value;
        public string Type;
    }

    public class PickleStepProcessor : StepProcessorBase
    {
        private StepStartedEvent stepStartedEvent;

        public PickleStepProcessor(ScenarioEventProcessor parentScenarioState) : base(parentScenarioState)
        {
        }

        public string PickleStepID { get; set; }
        public bool Bound { get; set; }
        public string CanonicalizedStepPattern { get; set; }
        public string StepDefinitionId { get; private set; }
        public IStepDefinitionBinding StepDefinition { get; set; }

        public StepArgument[] StepArguments { get; set; }

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
            StepDefinition = stepFinishedEvent.StepContext.StepInfo.BindingMatch.StepBinding;
            Bound = !(StepDefinition == null || StepDefinition == BindingMatch.NonMatching);

            if (Bound)
            {
                CanonicalizedStepPattern = CucumberMessageFactory.CanonicalizeStepDefinitionPattern(StepDefinition);
                StepDefinitionId = FindStepDefIDByStepPattern(CanonicalizedStepPattern);

                PickleStepID = FindPickleStepIDByStepText(stepFinishedEvent.StepContext.StepInfo.Text);

                Duration = stepFinishedEvent.Timestamp - stepStartedEvent.Timestamp;
                Status = stepFinishedEvent.StepContext.Status;

                StepArguments = stepFinishedEvent.StepContext.StepInfo.BindingMatch.Arguments.Select(arg => new StepArgument
                {
                    Value = arg.ToString(),
                    Type = arg.GetType().Name
                }).ToArray();
            }
            return Enumerable.Empty<Envelope>();
        }

    }

  
}