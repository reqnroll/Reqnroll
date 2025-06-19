using Gherkin.CucumberMessages;
using Io.Cucumber.Messages.Types;
using Reqnroll.Bindings;
using Reqnroll.BoDi;
using Reqnroll.Formatters.ExecutionTracking;
using System;

namespace Reqnroll.Formatters.PayloadProcessing.Cucumber;

public interface ICucumberMessageFactory
{
    // Core message methods
    TestRunStarted ToTestRunStarted(DateTime timestamp, string id);
    TestRunFinished ToTestRunFinished(bool testRunStatus, DateTime timestamp, string testRunStartedId);
    TestCase ToTestCase(TestCaseDefinition testCaseDefinition);
    TestCaseStarted ToTestCaseStarted(TestCaseExecutionRecord testCaseExecution, string testCaseId);
    TestCaseFinished ToTestCaseFinished(TestCaseExecutionRecord testCaseExecution);
    
    // Step definition methods
    StepDefinition ToStepDefinition(IStepDefinitionBinding binding, IIdGenerator idGenerator);
    StepDefinitionPattern ToStepDefinitionPattern(IStepDefinitionBinding binding);
    UndefinedParameterType ToUndefinedParameterType(string expression, string paramName, IIdGenerator idGenerator);
    ParameterType ToParameterType(IStepArgumentTransformationBinding stepTransform, IIdGenerator idGenerator);
    
    // Test step methods
    TestStep ToPickleTestStep(TestStepDefinition stepDef);
    StepMatchArgument ToStepMatchArgument(TestStepArgument argument);
    TestStepStarted ToTestStepStarted(TestStepTracker stepState);
    TestStepFinished ToTestStepFinished(TestStepTracker stepState);

    // Hook methods
    TestRunHookStarted ToTestRunHookStarted(TestRunHookTracker hookTracker);
    TestRunHookFinished ToTestRunHookFinished(TestRunHookTracker hookTracker);
    Hook ToHook(IHookBinding hookBinding, IIdGenerator iDGenerator);
    Io.Cucumber.Messages.Types.HookType ToHookType(IHookBinding hookBinding);
    TestStep ToHookTestStep(HookStepDefinition hookStepDefinition);
    TestStepStarted ToTestStepStarted(HookStepTracker hookStepProcessor);
    TestStepFinished ToTestStepFinished(HookStepTracker hookStepProcessor);
    
    // Attachment methods
    Attachment ToAttachment(AttachmentAddedEventWrapper tracker);
    Attachment ToAttachment(OutputAddedEventWrapper tracker);
    
    // Metadata methods
    Meta ToMeta(IObjectContainer container);
    
    // Utility methods
    string CanonicalizeStepDefinitionPattern(IStepDefinitionBinding stepDefinition);
    string CanonicalizeHookBinding(IHookBinding hookBinding);
}
