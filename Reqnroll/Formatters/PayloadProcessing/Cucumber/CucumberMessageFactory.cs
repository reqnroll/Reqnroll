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

    public virtual TestRunHookStarted ToTestRunHookStarted(TestRunHookTracker hookTracker)
    {
        return innerFactory.ToTestRunHookStarted(hookTracker);
    }

    public virtual TestRunHookFinished ToTestRunHookFinished(TestRunHookTracker hookTracker)
    {
        return innerFactory.ToTestRunHookFinished(hookTracker);
    }

    public virtual TestCase ToTestCase(TestCaseDefinition testCaseDefinition)
    {
        return innerFactory.ToTestCase(testCaseDefinition);
    }

    public virtual TestCaseStarted ToTestCaseStarted(TestCaseExecutionRecord testCaseExecution, string testCaseId)
    {
        return innerFactory.ToTestCaseStarted(testCaseExecution, testCaseId);
    }
    public virtual TestCaseFinished ToTestCaseFinished(TestCaseExecutionRecord testCaseExecution)
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

    public virtual TestStep ToPickleTestStep(TestStepDefinition stepDef)
    {
        return innerFactory.ToPickleTestStep(stepDef);
    }

    public virtual StepMatchArgument ToStepMatchArgument(TestStepArgument argument)
    {
        return innerFactory.ToStepMatchArgument(argument);
    }
    public virtual TestStepStarted ToTestStepStarted(TestStepTracker stepState)
    {
        return innerFactory.ToTestStepStarted(stepState);
    }

    public virtual TestStepFinished ToTestStepFinished(TestStepTracker stepState)
    {
        return innerFactory.ToTestStepFinished(stepState);
    }

    public virtual Hook ToHook(IHookBinding hookBinding, IIdGenerator iDGenerator)
    {
        return innerFactory.ToHook(hookBinding, iDGenerator);
    }

    public virtual Io.Cucumber.Messages.Types.HookType ToHookType(IHookBinding hookBinding)
    {
        return innerFactory.ToHookType(hookBinding);
    }

    public virtual TestStep ToHookTestStep(HookStepDefinition hookStepDefinition)
    {
        return innerFactory.ToHookTestStep(hookStepDefinition);
    }
    public virtual TestStepStarted ToTestStepStarted(HookStepTracker hookStepProcessor)
    {
        return innerFactory.ToTestStepStarted(hookStepProcessor);
    }

    public virtual TestStepFinished ToTestStepFinished(HookStepTracker hookStepProcessor)
    {
        return innerFactory.ToTestStepFinished(hookStepProcessor);
    }

    public virtual Attachment ToAttachment(AttachmentAddedEventWrapper tracker)
    {
        return innerFactory.ToAttachment(tracker);
    }
    public virtual Attachment ToAttachment(OutputAddedEventWrapper tracker)
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