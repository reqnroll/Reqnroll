using Cucumber.Messages;
using Gherkin.CucumberMessages;
using Io.Cucumber.Messages.Types;
using Reqnroll.Bindings;
using Reqnroll.Events;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Reqnroll.CucumberMesssages
{
    internal class CucumberMessageFactory
    {
        public static TestRunStarted ToTestRunStarted(FeatureState featureState, FeatureStartedEvent featureStartedEvent)
        {
            return new TestRunStarted(Converters.ToTimestamp(featureStartedEvent.Timestamp));
        }

        public static TestRunFinished ToTestRunFinished(FeatureState featureState, FeatureFinishedEvent featureFinishedEvent)
        {
            return new TestRunFinished(null, featureState.Success, Converters.ToTimestamp(featureFinishedEvent.Timestamp), null);
        }
        internal static TestCase ToTestCase(ScenarioState scenarioState, ScenarioStartedEvent scenarioStartedEvent)
        {
            var testSteps = new List<TestStep>();

            foreach (var stepState in scenarioState.Steps)
            {
                var testStep = CucumberMessageFactory.ToTestStep(scenarioState, stepState);
                testSteps.Add(testStep);
            }
            var testCase = new TestCase
            (
                scenarioState.TestCaseID,
                scenarioState.PickleID,
                testSteps
            );
            return testCase;
        }
        internal static TestCaseStarted ToTestCaseStarted(ScenarioState scenarioState, ScenarioStartedEvent scenarioStartedEvent)
        {
            return new TestCaseStarted(0, scenarioState.TestCaseStartedID, scenarioState.TestCaseID, null, Converters.ToTimestamp(scenarioStartedEvent.Timestamp));
        }
        internal static TestCaseFinished ToTestCaseFinished(ScenarioState scenarioState, ScenarioFinishedEvent scenarioFinishedEvent)
        {
            return new TestCaseFinished(scenarioState.TestCaseStartedID, Converters.ToTimestamp(scenarioFinishedEvent.Timestamp), false);
        }
        internal static StepDefinition ToStepDefinition(IStepDefinitionBinding binding, IIdGenerator idGenerator)
        {
            var bindingSourceText = binding.SourceExpression;
            var expressionType = binding.ExpressionType;
            var stepDefinitionPatternType = expressionType switch { StepDefinitionExpressionTypes.CucumberExpression => StepDefinitionPatternType.CUCUMBER_EXPRESSION, _ => StepDefinitionPatternType.REGULAR_EXPRESSION };
            var stepDefinitionPattern = new StepDefinitionPattern(bindingSourceText, stepDefinitionPatternType);

            var methodName = binding.Method.Name;
            var className = binding.Method.Type.Name;
            var paramTypes = binding.Method.Parameters.Select(x => x.Type.Name).ToList();
            var methodDescription = new JavaMethod(className, className, paramTypes);
            var sourceRef = SourceReference.Create(methodDescription);

            var result = new StepDefinition
            (
                idGenerator.GetNewId(),
                stepDefinitionPattern,
                sourceRef
            );
            return result;
        }

        internal static TestStep ToTestStep(ScenarioState scenarioState, StepState stepState)
        {
            //TODO: This only works if the step is properly bound. Need to determine what to do otherwise

             var args = stepState.StepArguments
                .Select(arg => CucumberMessageFactory.ToStepMatchArgument(arg))
                .ToList();

           var result = new TestStep(
                null,
                stepState.TestStepID,
                stepState.PickleStepID,
                new List<string> { stepState.StepDefinitionId },
                new List<StepMatchArgumentsList> { new StepMatchArgumentsList(args) }
                );

            return result;
        }

        internal static StepMatchArgument ToStepMatchArgument(StepArgument argument)
        {
            return new StepMatchArgument(
                new Group(
                    null,
                    null,
                    argument.Value
                    ),
                argument.Type);
        }
        internal static TestStepStarted ToTestStepStarted(StepState stepState, StepStartedEvent stepStartedEvent)
        {
            return new TestStepStarted(
                stepState.TestCaseStartedID,
                stepState.TestStepID,
                Converters.ToTimestamp(stepStartedEvent.Timestamp));
        }

        internal static TestStepFinished ToTestStepFinished(StepState stepState, StepFinishedEvent stepFinishedEvent)
        {
            return new TestStepFinished(
                stepState.TestCaseStartedID,
                stepState.TestStepID,
                ToTestStepResult(stepState, stepFinishedEvent),
                Converters.ToTimestamp(stepFinishedEvent.Timestamp));
        }

        private static TestStepResult ToTestStepResult(StepState stepState, StepFinishedEvent stepFinishedEvent)
        {
            return new TestStepResult(
                Converters.ToDuration(stepState.Duration),
                "",
                ToTestStepResultStatus(stepState.Status),
                null);

        }

        private static TestStepResultStatus ToTestStepResultStatus(ScenarioExecutionStatus status)
        {
            return status switch
            {
                ScenarioExecutionStatus.OK => TestStepResultStatus.PASSED,
                ScenarioExecutionStatus.BindingError => TestStepResultStatus.AMBIGUOUS,
                ScenarioExecutionStatus.TestError => TestStepResultStatus.FAILED,
                ScenarioExecutionStatus.Skipped => TestStepResultStatus.SKIPPED,
                ScenarioExecutionStatus.UndefinedStep => TestStepResultStatus.UNDEFINED,
                ScenarioExecutionStatus.StepDefinitionPending => TestStepResultStatus.PENDING,
                _ => TestStepResultStatus.UNKNOWN
            };
        }


        // utility methods
        public static string CanonicalizeStepDefinitionPattern(IStepDefinitionBinding stepDefinition)
        {

            var signature = stepDefinition.Method != null ? String.Join(",", stepDefinition.Method.Parameters.Select(p => p.Type.Name)) : "";

            return $"{stepDefinition.SourceExpression}({signature})";
        }

    }
}