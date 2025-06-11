using Cucumber.Messages;
using Gherkin.CucumberMessages;
using Io.Cucumber.Messages.Types;
using Reqnroll.Bindings;
using Reqnroll.BoDi;
using Reqnroll.CommonModels;
using Reqnroll.Formatters.ExecutionTracking;
using Reqnroll.EnvironmentAccess;
using Reqnroll.Events;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace Reqnroll.Formatters.PayloadProcessing.Cucumber
{
    /// <summary>
    /// This class provides functions to convert execution level detail into Cucumber message elements
    /// 
    /// These are typically called after execution is completed for a Feature.
    /// 
    /// This class leverages an 'inner' implementation class that can be swapped out for a mock during testing
    /// </summary>
    public class CucumberMessageFactory : ICucumberMessageFactory
    {
        public CucumberMessageFactory() : this(new CucumberMessageFactoryInner()) { }
        public CucumberMessageFactory(ICucumberMessageFactory innerfactory)
        {
            inner = innerfactory;
        }
        internal ICucumberMessageFactory inner;
        public virtual TestRunStarted ToTestRunStarted(DateTime timestamp, string id)
        {
            return inner.ToTestRunStarted(timestamp, id);
        }

        public virtual TestRunFinished ToTestRunFinished(bool testRunStatus, DateTime timestamp, string testRunStartedId)
        {
            return inner.ToTestRunFinished(testRunStatus, timestamp, testRunStartedId);
        }

        public virtual TestRunHookStarted ToTestRunHookStarted(TestRunHookTracker hookTracker)
        {
            return inner.ToTestRunHookStarted(hookTracker);
        }

        public virtual TestRunHookFinished ToTestRunHookFinished(TestRunHookTracker hookTracker)
        {
            return inner.ToTestRunHookFinished(hookTracker);
        }

        public virtual TestCase ToTestCase(TestCaseDefinition testCaseDefinition)
        {
            return inner.ToTestCase(testCaseDefinition);
        }

        public virtual TestCaseStarted ToTestCaseStarted(TestCaseExecutionRecord testCaseExecution, string testCaseId)
        {
            return inner.ToTestCaseStarted(testCaseExecution, testCaseId);
        }
        public virtual TestCaseFinished ToTestCaseFinished(TestCaseExecutionRecord testCaseExecution)
        {
            return inner.ToTestCaseFinished(testCaseExecution);
        }
        public virtual StepDefinition ToStepDefinition(IStepDefinitionBinding binding, IIdGenerator idGenerator)
        {
            return inner.ToStepDefinition(binding, idGenerator);
        }

        public virtual StepDefinitionPattern ToStepDefinitionPattern(IStepDefinitionBinding binding)
        {
            return inner.ToStepDefinitionPattern(binding);
        }
        public virtual UndefinedParameterType ToUndefinedParameterType(string expression, string paramName, IIdGenerator iDGenerator)
        {
            return inner.ToUndefinedParameterType(expression, paramName, iDGenerator);
        }

        public virtual ParameterType ToParameterType(IStepArgumentTransformationBinding stepTransform, IIdGenerator iDGenerator)
        {
            return inner.ToParameterType(stepTransform, iDGenerator);
        }

        public virtual TestStep ToPickleTestStep(TestStepDefinition stepDef)
        {
            return inner.ToPickleTestStep(stepDef);
        }

        public virtual StepMatchArgument ToStepMatchArgument(TestStepArgument argument)
        {
            return inner.ToStepMatchArgument(argument);
        }
        public virtual TestStepStarted ToTestStepStarted(TestStepTracker stepState)
        {
            return inner.ToTestStepStarted(stepState);
        }

        public virtual TestStepFinished ToTestStepFinished(TestStepTracker stepState)
        {
            return inner.ToTestStepFinished(stepState);
        }

        public virtual Hook ToHook(IHookBinding hookBinding, IIdGenerator iDGenerator)
        {
            return inner.ToHook(hookBinding, iDGenerator);
        }

        public virtual Io.Cucumber.Messages.Types.HookType ToHookType(IHookBinding hookBinding)
        {
            return inner.ToHookType(hookBinding);
        }

        public virtual TestStep ToHookTestStep(HookStepDefinition hookStepDefinition)
        {
            return inner.ToHookTestStep(hookStepDefinition);
        }
        public virtual TestStepStarted ToTestStepStarted(HookStepTracker hookStepProcessor)
        {
            return inner.ToTestStepStarted(hookStepProcessor);
        }

        public virtual TestStepFinished ToTestStepFinished(HookStepTracker hookStepProcessor)
        {
            return inner.ToTestStepFinished(hookStepProcessor);
        }

        public virtual Attachment ToAttachment(AttachmentAddedEventWrapper tracker)
        {
            return inner.ToAttachment(tracker);
        }
        public virtual Attachment ToAttachment(OutputAddedEventWrapper tracker)
        {
            return inner.ToAttachment(tracker);
        }

        public virtual Meta ToMeta(IObjectContainer container)
        {
            return inner.ToMeta(container);
        }

        #region utility methods
        public virtual string CanonicalizeStepDefinitionPattern(IStepDefinitionBinding stepDefinition)
        {
            return inner.CanonicalizeStepDefinitionPattern(stepDefinition);
        }

        public virtual string CanonicalizeHookBinding(IHookBinding hookBinding)
        {
            return inner.CanonicalizeHookBinding(hookBinding);
        }
        #endregion
    }
}