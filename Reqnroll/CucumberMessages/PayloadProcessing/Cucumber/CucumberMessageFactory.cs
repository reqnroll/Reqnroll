using Cucumber.Messages;
using Gherkin.CucumberMessages;
using Io.Cucumber.Messages.Types;
using Reqnroll.Bindings;
using Reqnroll.BoDi;
using Reqnroll.CommonModels;
using Reqnroll.CucumberMessages.ExecutionTracking;
using Reqnroll.EnvironmentAccess;
using Reqnroll.Events;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace Reqnroll.CucumberMessages.PayloadProcessing.Cucumber
{
    /// <summary>
    /// This class provides functions to convert execution level detail into Cucumber message elements
    /// 
    /// These are typically called after execution is completed for a Feature.
    /// 
    /// This class leverages an 'inner' implementation class that can be swapped out for a mock during testing
    /// </summary>
    internal class CucumberMessageFactory
    {
        internal static CucumberMessageFactoryInner inner = new();
        internal static TestRunStarted ToTestRunStarted(DateTime timestamp, string id)
        {
            return inner.ToTestRunStarted(timestamp, id);
        }

        internal static TestRunFinished ToTestRunFinished(bool testRunStatus, DateTime timestamp, string testRunStartedId)
        {
            return inner.ToTestRunFinished(testRunStatus, timestamp, testRunStartedId);
        }

        internal static TestRunHookStarted ToTestRunHookStarted(TestRunHookTracker hookTracker)
        {
            return inner.ToTestRunHookStarted(hookTracker);
        }

        internal static TestRunHookFinished ToTestRunHookFinished(TestRunHookTracker hookTracker)
        {
            return inner.ToTestRunHookFinished(hookTracker);
        }

        internal static TestCase ToTestCase(TestCaseDefinition testCaseDefinition)
        {
            return inner.ToTestCase(testCaseDefinition);
        }

        internal static TestCaseStarted ToTestCaseStarted(TestCaseExecutionRecord testCaseExecution, string testCaseId)
        {
            return inner.ToTestCaseStarted(testCaseExecution, testCaseId);
        }
        internal static TestCaseFinished ToTestCaseFinished(TestCaseExecutionRecord testCaseExecution)
        {
            return inner.ToTestCaseFinished(testCaseExecution);
        }
        internal static StepDefinition ToStepDefinition(IStepDefinitionBinding binding, IIdGenerator idGenerator)
        {
            return inner.ToStepDefinition(binding, idGenerator);
        }

        internal static StepDefinitionPattern ToStepDefinitionPattern(IStepDefinitionBinding binding)
        {
            return inner.ToStepDefinitionPattern(binding);
        }
        internal static UndefinedParameterType ToUndefinedParameterType(string expression, string paramName, IIdGenerator iDGenerator)
        {
            return inner.ToUndefinedParameterType(expression, paramName, iDGenerator);
        }

        internal static ParameterType ToParameterType(IStepArgumentTransformationBinding stepTransform, IIdGenerator iDGenerator)
        {
            return inner.ToParameterType(stepTransform, iDGenerator);
        }

        internal static TestStep ToPickleTestStep(TestStepDefinition stepDef)
        {
            return inner.ToPickleTestStep(stepDef);
        }

        internal static StepMatchArgument ToStepMatchArgument(TestStepArgument argument)
        {
            return inner.ToStepMatchArgument(argument);
        }
        internal static TestStepStarted ToTestStepStarted(TestStepTracker stepState)
        {
            return inner.ToTestStepStarted(stepState);
        }

        internal static TestStepFinished ToTestStepFinished(TestStepTracker stepState)
        {
            return inner.ToTestStepFinished(stepState);
        }

        internal static Hook ToHook(IHookBinding hookBinding, IIdGenerator iDGenerator)
        {
            return inner.ToHook(hookBinding, iDGenerator);
        }

        internal static Io.Cucumber.Messages.Types.HookType ToHookType(IHookBinding hookBinding)
        {
            return inner.ToHookType(hookBinding);
        }

        internal static TestStep ToHookTestStep(HookStepDefinition hookStepDefinition)
        {
            return inner.ToHookTestStep(hookStepDefinition);
        }
        internal static TestStepStarted ToTestStepStarted(HookStepTracker hookStepProcessor)
        {
            return inner.ToTestStepStarted(hookStepProcessor);
        }

        internal static TestStepFinished ToTestStepFinished(HookStepTracker hookStepProcessor)
        {
            return inner.ToTestStepFinished(hookStepProcessor);
        }

        internal static Attachment ToAttachment(AttachmentAddedEventWrapper tracker)
        {
            return inner.ToAttachment(tracker);
        }
        internal static Attachment ToAttachment(OutputAddedEventWrapper tracker)
        {
            return inner.ToAttachment(tracker);
        }

        internal static Meta ToMeta(IObjectContainer container)
        {
            return inner.ToMeta(container);
        }

        #region utility methods
        internal static string CanonicalizeStepDefinitionPattern(IStepDefinitionBinding stepDefinition)
        {
            return inner.CanonicalizeStepDefinitionPattern(stepDefinition);
        }

        internal static string CanonicalizeHookBinding(IHookBinding hookBinding)
        {
            return inner.CanonicalizeHookBinding(hookBinding);
        }
        #endregion
    }
}