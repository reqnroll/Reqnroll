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
        internal static TestCase ToTestCase(ScenarioState scenarioState, ScenarioStartedEvent scenarioStartedEvent)
        {
            var testCase = new TestCase
            (
                scenarioState.TestCaseID,
                scenarioState.PickleID,
                new List<TestStep>()
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
    }
}