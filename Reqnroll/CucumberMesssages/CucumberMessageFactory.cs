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
        public static TestRunStarted ToTestRunStarted(FeatureEventProcessor featureState, FeatureStartedEvent featureStartedEvent)
        {
            return new TestRunStarted(Converters.ToTimestamp(featureStartedEvent.Timestamp));
        }

        public static TestRunFinished ToTestRunFinished(FeatureEventProcessor featureState, FeatureFinishedEvent featureFinishedEvent)
        {
            return new TestRunFinished(null, featureState.Success, Converters.ToTimestamp(featureFinishedEvent.Timestamp), null);
        }
        internal static TestCase ToTestCase(ScenarioEventProcessor scenarioState, ScenarioStartedEvent scenarioStartedEvent)
        {
            var testSteps = new List<TestStep>();

            foreach (var stepState in scenarioState.Steps)
            {
                switch (stepState)
                {
                    case PickleStepProcessor _:
                        var testStep = CucumberMessageFactory.ToTestStep(scenarioState, stepState as PickleStepProcessor);
                        testSteps.Add(testStep);
                        break;
                    case HookStepProcessor _:
                        var hookTestStep = CucumberMessageFactory.ToHookTestStep( stepState as HookStepProcessor);
                        testSteps.Add(hookTestStep);
                        break;
                    default:
                        throw new NotImplementedException();
                }
            }
            var testCase = new TestCase
            (
                scenarioState.TestCaseID,
                scenarioState.PickleID,
                testSteps
            );
            return testCase;
        }
        internal static TestCaseStarted ToTestCaseStarted(ScenarioEventProcessor scenarioState, ScenarioStartedEvent scenarioStartedEvent)
        {
            return new TestCaseStarted(0, scenarioState.TestCaseStartedID, scenarioState.TestCaseID, null, Converters.ToTimestamp(scenarioStartedEvent.Timestamp));
        }
        internal static TestCaseFinished ToTestCaseFinished(ScenarioEventProcessor scenarioState, ScenarioFinishedEvent scenarioFinishedEvent)
        {
            return new TestCaseFinished(scenarioState.TestCaseStartedID, Converters.ToTimestamp(scenarioFinishedEvent.Timestamp), false);
        }
        internal static StepDefinition ToStepDefinition(IStepDefinitionBinding binding, IIdGenerator idGenerator)
        {
            var bindingSourceText = binding.SourceExpression;
            var expressionType = binding.ExpressionType;
            var stepDefinitionPatternType = expressionType switch { StepDefinitionExpressionTypes.CucumberExpression => StepDefinitionPatternType.CUCUMBER_EXPRESSION, _ => StepDefinitionPatternType.REGULAR_EXPRESSION };
            var stepDefinitionPattern = new StepDefinitionPattern(bindingSourceText, stepDefinitionPatternType);
            SourceReference sourceRef = ToSourceRef(binding);

            var result = new StepDefinition
            (
                idGenerator.GetNewId(),
                stepDefinitionPattern,
                sourceRef
            );
            return result;
        }

        private static SourceReference ToSourceRef(IBinding binding)
        {
            var methodName = binding.Method.Name;
            var className = binding.Method.Type.Name;
            var paramTypes = binding.Method.Parameters.Select(x => x.Type.Name).ToList();
            var methodDescription = new JavaMethod(className, methodName, paramTypes);
            var sourceRef = SourceReference.Create(methodDescription);
            return sourceRef;
        }

        internal static TestStep ToTestStep(ScenarioEventProcessor scenarioState, PickleStepProcessor stepState)
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
                    new List<Group>(),
                    null,
                    argument.Value
                    ),
                argument.Type);
        }
        internal static TestStepStarted ToTestStepStarted(PickleStepProcessor stepState, StepStartedEvent stepStartedEvent)
        {
            return new TestStepStarted(
                stepState.TestCaseStartedID,
                stepState.TestStepID,
                Converters.ToTimestamp(stepStartedEvent.Timestamp));
        }

        internal static TestStepFinished ToTestStepFinished(PickleStepProcessor stepState, StepFinishedEvent stepFinishedEvent)
        {
            return new TestStepFinished(
                stepState.TestCaseStartedID,
                stepState.TestStepID,
                ToTestStepResult(stepState),
                Converters.ToTimestamp(stepFinishedEvent.Timestamp));
        }

        internal static Hook ToHook(IHookBinding hookBinding, IIdGenerator iDGenerator)
        {
            SourceReference sourceRef = ToSourceRef(hookBinding);

            var result = new Hook
            (
                iDGenerator.GetNewId(),
                null,
                sourceRef,
                hookBinding.IsScoped ? hookBinding.BindingScope.Tag : null
            );
            return result;
        }

        internal static TestStep ToHookTestStep(HookStepProcessor hookStepState)
        {
            // find the Hook message at the Feature level
            var hookCacheKey = CanonicalizeHookBinding(hookStepState.HookBindingFinishedEvent.HookBinding);
            var hookId = hookStepState.parentScenario.FeatureState.HookDefinitionsByPattern[hookCacheKey];

            return new TestStep(hookId, hookStepState.TestStepID, null, new List<string>(), new List<StepMatchArgumentsList>());
        }
        internal static TestStepStarted ToTestStepStarted(HookStepProcessor hookStepProcessor, HookBindingFinishedEvent hookBindingFinishedEvent)
        {
            return new TestStepStarted(hookStepProcessor.TestCaseStartedID, hookStepProcessor.TestStepID, Converters.ToTimestamp(hookBindingFinishedEvent.Timestamp));
        }

        internal static TestStepFinished ToTestStepFinished(HookStepProcessor hookStepProcessor, HookBindingFinishedEvent hookBindingFinishedEvent)
        {
            return new TestStepFinished(hookStepProcessor.TestCaseStartedID, hookStepProcessor.TestStepID, ToTestStepResult(hookStepProcessor), Converters.ToTimestamp(hookBindingFinishedEvent.Timestamp));
        }


        private static TestStepResult ToTestStepResult(StepProcessorBase stepState)
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
            string signature = GenerateSignature(stepDefinition);

            return $"{stepDefinition.SourceExpression}({signature})";
        }

        public static string CanonicalizeHookBinding(IHookBinding hookBinding)
        {
            string signature = GenerateSignature(hookBinding);
            return $"{hookBinding.Method.Type.Name}.{hookBinding.Method.Name}({signature})";
        }

        private static string GenerateSignature(IBinding stepDefinition)
        {
            return stepDefinition.Method != null ? String.Join(",", stepDefinition.Method.Parameters.Select(p => p.Type.Name)) : "";
        }

    }
}