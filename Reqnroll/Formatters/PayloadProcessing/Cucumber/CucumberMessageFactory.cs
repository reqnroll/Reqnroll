using Gherkin.CucumberMessages;
using Io.Cucumber.Messages.Types;
using Reqnroll.Bindings;
using Reqnroll.BoDi;
using Reqnroll.Formatters.ExecutionTracking;
using System;

namespace Reqnroll.Formatters.PayloadProcessing.Cucumber;

/// <summary>
/// This class provides functions to convert execution level detail into Cucumber message elements.
/// These are typically called after execution is completed for a Feature.
/// This class leverages an 'innerFactory' implementation class that can be swapped out for a mock during testing.
/// </summary>
public class CucumberMessageFactory(ICucumberMessageFactory innerFactory) : ICucumberMessageFactory
{
    public CucumberMessageFactory() : this(new CucumberMessageFactoryInner()) { }

    public virtual TestRunStarted ToTestRunStarted(DateTime timestamp, string id)
    {
        return innerFactory.ToTestRunStarted(timestamp, id);
    }

    public virtual TestRunFinished ToTestRunFinished(bool testRunStatus, DateTime timestamp, string testRunStartedId)
    {
        return innerFactory.ToTestRunFinished(testRunStatus, timestamp, testRunStartedId);
    }

    public virtual TestRunHookStarted ToTestRunHookStarted(TestRunHookExecutionTracker hookExecutionTracker)
    {
        return innerFactory.ToTestRunHookStarted(hookExecutionTracker);
    }

    public virtual TestRunHookFinished ToTestRunHookFinished(TestRunHookExecutionTracker hookExecutionTracker)
    {
        return innerFactory.ToTestRunHookFinished(hookExecutionTracker);
    }

    public virtual TestCase ToTestCase(TestCaseTracker testCaseTracker)
    {
        return innerFactory.ToTestCase(testCaseTracker);
    }

    public virtual TestCaseStarted ToTestCaseStarted(TestCaseExecutionTracker testCaseExecution, string testCaseId)
    {
        return innerFactory.ToTestCaseStarted(testCaseExecution, testCaseId);
    }
    public virtual TestCaseFinished ToTestCaseFinished(TestCaseExecutionTracker testCaseExecution)
    {
        return innerFactory.ToTestCaseFinished(testCaseExecution);
    }
    public virtual StepDefinition ToStepDefinition(IStepDefinitionBinding binding, IIdGenerator idGenerator)
    {
        return innerFactory.ToStepDefinition(binding, idGenerator);
    }

    public virtual StepDefinitionPattern ToStepDefinitionPattern(IStepDefinitionBinding binding)
    {
        return innerFactory.ToStepDefinitionPattern(binding);
    }
    public virtual UndefinedParameterType ToUndefinedParameterType(string expression, string paramName, IIdGenerator iDGenerator)
    {
        return innerFactory.ToUndefinedParameterType(expression, paramName, iDGenerator);
    }

    public virtual ParameterType ToParameterType(IStepArgumentTransformationBinding stepTransform, IIdGenerator iDGenerator)
    {
        return innerFactory.ToParameterType(stepTransform, iDGenerator);
    }

    public virtual TestStep ToTestStep(TestStepTracker stepDef)
    {
        return innerFactory.ToTestStep(stepDef);
    }

    public virtual StepMatchArgument ToStepMatchArgument(TestStepArgument argument)
    {
        return innerFactory.ToStepMatchArgument(argument);
    }
    public virtual TestStepStarted ToTestStepStarted(TestStepExecutionTracker testStepExecutionTracker)
    {
        return innerFactory.ToTestStepStarted(testStepExecutionTracker);
    }

    public virtual TestStepFinished ToTestStepFinished(TestStepExecutionTracker testStepExecutionTracker)
    {
        return innerFactory.ToTestStepFinished(testStepExecutionTracker);
    }

    public virtual Hook ToHook(IHookBinding hookBinding, IIdGenerator iDGenerator)
    {
        return innerFactory.ToHook(hookBinding, iDGenerator);
    }

    public virtual Io.Cucumber.Messages.Types.HookType ToHookType(IHookBinding hookBinding)
    {
        return innerFactory.ToHookType(hookBinding);
    }

    public virtual TestStep ToTestStep(HookStepTracker hookStepTracker)
    {
        return innerFactory.ToTestStep(hookStepTracker);
    }
    public virtual TestStepStarted ToTestStepStarted(HookStepExecutionTracker hookStepExecutionTracker)
    {
        return innerFactory.ToTestStepStarted(hookStepExecutionTracker);
    }

    public virtual TestStepFinished ToTestStepFinished(HookStepExecutionTracker hookStepExecutionTracker)
    {
        return innerFactory.ToTestStepFinished(hookStepExecutionTracker);
    }

    public virtual Attachment ToAttachment(AttachmentTracker tracker)
    {
        return innerFactory.ToAttachment(tracker);
    }
    public virtual Attachment ToAttachment(OutputMessageTracker tracker)
    {
        return innerFactory.ToAttachment(tracker);
    }

    public virtual Meta ToMeta(IObjectContainer container)
    {
        return innerFactory.ToMeta(container);
    }

    #region utility methods
    public virtual string CanonicalizeStepDefinitionPattern(IStepDefinitionBinding stepDefinition)
    {
        return innerFactory.CanonicalizeStepDefinitionPattern(stepDefinition);
    }

    public virtual string CanonicalizeHookBinding(IHookBinding hookBinding)
    {
        return innerFactory.CanonicalizeHookBinding(hookBinding);
    }
    #endregion
}