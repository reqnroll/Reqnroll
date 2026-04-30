using Gherkin.CucumberMessages;
using Io.Cucumber.Messages.Types;
using Reqnroll.Bindings;
using Reqnroll.EnvironmentAccess;
using Reqnroll.Formatters.Configuration;
using Reqnroll.Formatters.ExecutionTracking;
using System;
using System.Collections.Generic;

namespace Reqnroll.Formatters.PayloadProcessing.Cucumber;

public interface ICucumberMessageFactory
{
    // Core message methods
    TestRunStarted ToTestRunStarted(DateTime timestamp, string id);
    TestRunFinished ToTestRunFinished(bool testRunStatus, DateTime timestamp, string testRunStartedId);
    TestCase ToTestCase(TestCaseTracker testCaseTracker);
    TestCaseStarted ToTestCaseStarted(TestCaseExecutionTracker testCaseExecution, string testCaseId);
    TestCaseFinished ToTestCaseFinished(TestCaseExecutionTracker testCaseExecution, bool willBeRetried = false);
    
    // Step definition methods
    StepDefinition ToStepDefinition(IStepDefinitionBinding binding, IIdGenerator idGenerator);
    StepDefinitionPattern ToStepDefinitionPattern(IStepDefinitionBinding binding);
    UndefinedParameterType ToUndefinedParameterType(string expression, string paramName, IIdGenerator idGenerator);
    ParameterType ToParameterType(IStepArgumentTransformationBinding stepTransform, IIdGenerator idGenerator);
    
    // Test step methods
    TestStep ToTestStep(TestStepTracker stepDef);
    StepMatchArgument ToStepMatchArgument(TestStepArgument argument);
    TestStepStarted ToTestStepStarted(TestStepExecutionTracker testStepExecutionTracker);
    TestStepFinished ToTestStepFinished(TestStepExecutionTracker testStepExecutionTracker);
    Suggestion ToSuggestion(TestStepExecutionTracker testStepExecutionTracker, string programmingLanguage, string skeletonMessage, IIdGenerator idGenerator);

    // Hook methods
    TestRunHookStarted ToTestRunHookStarted(TestRunHookExecutionTracker hookExecutionTracker);
    TestRunHookFinished ToTestRunHookFinished(TestRunHookExecutionTracker hookExecutionTracker);
    Hook ToHook(IHookBinding hookBinding, IIdGenerator iDGenerator);
    Io.Cucumber.Messages.Types.HookType ToHookType(IHookBinding hookBinding);
    TestStep ToTestStep(HookStepTracker hookStepTracker);
    TestStepStarted ToTestStepStarted(HookStepExecutionTracker hookStepExecutionTracker);
    TestStepFinished ToTestStepFinished(HookStepExecutionTracker hookStepExecutionTracker);

    // Attachment methods

    bool TryCreateAttachmentEnvelope(AttachmentTracker tracker, out Envelope attachmentEnvelope);
    Attachment ToAttachment(AttachmentTracker tracker);
    ExternalAttachment ToExternalAttachment(AttachmentTracker tracker);

    // Creates either an <see cref="Attachment"/> or <see cref="ExternalAttachment"/> based on the <see cref="AttachmentHandlingOption"> specified by the <see cref="IUnitTestRuntimeProvider"/>. 
    object CreateAttachment(AttachmentTracker attachment);

    Attachment ToAttachment(OutputMessageTracker tracker);
  
    // Metadata methods
    Meta ToMeta(string reqnrollVersion, string netCoreVersion, string osPlatform, BuildMetadata buildMetaData);
    
    // Utility methods
    string CanonicalizeStepDefinitionPattern(IStepDefinitionBinding stepDefinition);
    string CanonicalizeHookBinding(IHookBinding hookBinding);

}