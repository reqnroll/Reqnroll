using Cucumber.Messages;
using Gherkin.CucumberMessages;
using Io.Cucumber.Messages.Types;
using Reqnroll.Bindings;
using Reqnroll.BoDi;
using Reqnroll.CommonModels;
using Reqnroll.Formatters.ExecutionTracking;
using Reqnroll.EnvironmentAccess;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using Group = Io.Cucumber.Messages.Types.Group;

namespace Reqnroll.Formatters.PayloadProcessing.Cucumber;

/// <summary>
/// This class provides functions to convert execution level detail into Cucumber message elements.
/// These are typically called after execution is completed for a Feature.
/// </summary>
public class CucumberMessageFactoryInner : ICucumberMessageFactory
{
    public virtual TestRunStarted ToTestRunStarted(DateTime timestamp, string id)
    {
        return new TestRunStarted(Converters.ToTimestamp(timestamp.ToUniversalTime()), id);
    }

    public virtual TestRunFinished ToTestRunFinished(bool testRunStatus, DateTime timestamp, string testRunStartedId)
    {
        return new TestRunFinished(null, testRunStatus, Converters.ToTimestamp(timestamp.ToUniversalTime()), null, testRunStartedId);
    }

    public virtual TestRunHookStarted ToTestRunHookStarted(TestRunHookTracker hookTracker)
    {
        return new TestRunHookStarted(hookTracker.TestRunHookId, hookTracker.TestRunID, hookTracker.TestRunHook_HookId, Converters.ToTimestamp(hookTracker.TimeStamp.ToUniversalTime()));
    }

    public virtual TestRunHookFinished ToTestRunHookFinished(TestRunHookTracker hookTracker)
    {
        return new TestRunHookFinished(hookTracker.TestRunHookId, ToTestStepResult(hookTracker), Converters.ToTimestamp(hookTracker.TimeStamp.ToUniversalTime()));
    }

    public virtual TestCase ToTestCase(TestCaseDefinition testCaseDefinition)
    {
        var testSteps = new List<TestStep>();

        foreach (var stepDefinition in testCaseDefinition.StepDefinitions)
        {
            switch (stepDefinition)
            {
                case HookStepDefinition hookStepDefinition:
                    var hookTestStep = ToHookTestStep(hookStepDefinition);
                    testSteps.Add(hookTestStep);
                    break;
                case TestStepDefinition _:
                    var testStep = ToPickleTestStep(stepDefinition);
                    testSteps.Add(testStep);
                    break;
                default:
                    throw new NotImplementedException();
            }
        }
        var testCase = new TestCase
        (
            testCaseDefinition.TestCaseId,
            testCaseDefinition.PickleId,
            testSteps,
            testCaseDefinition.Tracker.TestRunStartedId
        );
        return testCase;
    }

    public virtual TestCaseStarted ToTestCaseStarted(TestCaseExecutionRecord testCaseExecution, string testCaseId)
    {
        return new TestCaseStarted(
            testCaseExecution.AttemptId,
            testCaseExecution.TestCaseStartedId,
            testCaseId,
            null,
            Converters.ToTimestamp(testCaseExecution.TestCaseStartedTimeStamp.ToUniversalTime()));
    }

    public virtual TestCaseFinished ToTestCaseFinished(TestCaseExecutionRecord testCaseExecution)
    {
        return new TestCaseFinished(
            testCaseExecution.TestCaseStartedId,
            Converters.ToTimestamp(testCaseExecution.TestCaseFinishedTimeStamp.ToUniversalTime()),
            false);
    }

    public virtual StepDefinition ToStepDefinition(IStepDefinitionBinding binding, IIdGenerator idGenerator)
    {
        StepDefinitionPattern stepDefinitionPattern = ToStepDefinitionPattern(binding);
        SourceReference sourceRef = ToSourceRef(binding);

        var result = new StepDefinition
        (
            idGenerator.GetNewId(),
            stepDefinitionPattern,
            sourceRef
        );
        return result;
    }

    public virtual StepDefinitionPattern ToStepDefinitionPattern(IStepDefinitionBinding binding)
    {
        var bindingSourceText = binding.SourceExpression;
        var expressionType = binding.ExpressionType;
        var stepDefinitionPatternType = expressionType switch
        {
            StepDefinitionExpressionTypes.CucumberExpression => StepDefinitionPatternType.CUCUMBER_EXPRESSION,
            _ => StepDefinitionPatternType.REGULAR_EXPRESSION
        };
        var stepDefinitionPattern = new StepDefinitionPattern(bindingSourceText, stepDefinitionPatternType);
        return stepDefinitionPattern;
    }

    public virtual UndefinedParameterType ToUndefinedParameterType(string expression, string paramName, IIdGenerator iDGenerator)
    {
        return new UndefinedParameterType(expression, paramName);
    }

    public virtual ParameterType ToParameterType(IStepArgumentTransformationBinding stepTransform, IIdGenerator iDGenerator)
    {
        var regexPattern = stepTransform.Regex?.ToString();
        var name = stepTransform.Name ?? stepTransform.Method.ReturnType.Name;
        var result = new ParameterType
        (
            name,
            [regexPattern],
            false,
            false,
            iDGenerator.GetNewId(),
            ToSourceRef(stepTransform)
        );
        return result;
    }

    private static SourceReference ToSourceRef(IBinding binding)
    {
        var methodName = binding.Method.Name;
        var className = binding.Method.Type.AssemblyName + "." + binding.Method.Type.FullName;
        var paramTypes = binding.Method.Parameters.Select(x => x.Type.Name).ToList();
        var methodDescription = new JavaMethod(className, methodName, paramTypes);
        var sourceRef = SourceReference.Create(methodDescription);
        return sourceRef;
    }

    public virtual TestStep ToPickleTestStep(TestStepDefinition stepDef)
    {
        bool bound = stepDef.Bound;

        var args = stepDef.StepArguments
                          .Select(ToStepMatchArgument)
                          .ToList();

        var result = new TestStep(
            null,
            stepDef.TestStepId,
            stepDef.PickleStepID,
            stepDef.StepDefinitionIds,
            bound ? [new StepMatchArgumentsList(args)] : new List<StepMatchArgumentsList>()
        );

        return result;
    }

    public virtual StepMatchArgument ToStepMatchArgument(TestStepArgument argument)
    {
        return new StepMatchArgument(
            new Group(
                [],
                argument.StartOffset,
                argument.Value
            ),
            NormalizePrimitiveTypeNamesToCucumberTypeNames(argument.Type));
    }

    public virtual TestStepStarted ToTestStepStarted(TestStepTracker stepState)
    {
        return new TestStepStarted(
            stepState.TestCaseStartedID,
            stepState.Definition.TestStepId,
            Converters.ToTimestamp(stepState.StepStarted.ToUniversalTime()));
    }

    public virtual TestStepFinished ToTestStepFinished(TestStepTracker stepState)
    {
        return new TestStepFinished(
            stepState.TestCaseStartedID,
            stepState.Definition.TestStepId,
            ToTestStepResult(stepState),
            Converters.ToTimestamp(stepState.StepFinished.ToUniversalTime()));
    }

    public virtual Hook ToHook(IHookBinding hookBinding, IIdGenerator iDGenerator)
    {
        SourceReference sourceRef = ToSourceRef(hookBinding);

        var result = new Hook
        (
            iDGenerator.GetNewId(),
            null,
            sourceRef,
            hookBinding.IsScoped ? $"@{hookBinding.BindingScope.Tag}" : null,
            ToHookType(hookBinding)
        );
        return result;
    }

    public virtual Io.Cucumber.Messages.Types.HookType ToHookType(IHookBinding hookBinding)
    {
        return hookBinding.HookType switch
        {
            Bindings.HookType.BeforeTestRun => Io.Cucumber.Messages.Types.HookType.BEFORE_TEST_RUN,
            Bindings.HookType.AfterTestRun => Io.Cucumber.Messages.Types.HookType.AFTER_TEST_RUN,
            Bindings.HookType.BeforeFeature => Io.Cucumber.Messages.Types.HookType.BEFORE_TEST_RUN,
            Bindings.HookType.AfterFeature => Io.Cucumber.Messages.Types.HookType.AFTER_TEST_RUN,
            Bindings.HookType.BeforeScenario => Io.Cucumber.Messages.Types.HookType.BEFORE_TEST_CASE,
            Bindings.HookType.AfterScenario => Io.Cucumber.Messages.Types.HookType.AFTER_TEST_CASE,
            Bindings.HookType.BeforeStep => Io.Cucumber.Messages.Types.HookType.AFTER_TEST_STEP,
            Bindings.HookType.AfterStep => Io.Cucumber.Messages.Types.HookType.AFTER_TEST_STEP,

            // Note: The following isn't strictly correct, but about all that can be done given that Cucumber doesn't support any other types of Hooks
            _ => Io.Cucumber.Messages.Types.HookType.BEFORE_TEST_RUN
        };
    }

    public virtual TestStep ToHookTestStep(HookStepDefinition hookStepDefinition)
    {
        var hookId = hookStepDefinition.HookId;

        return new TestStep(
            hookId,
            hookStepDefinition.TestStepId,
            null,
            null,
            null);
    }

    public virtual TestStepStarted ToTestStepStarted(HookStepTracker hookStepProcessor)
    {
        return new TestStepStarted(hookStepProcessor.TestCaseStartedID,
                                   hookStepProcessor.Definition.TestStepId,
                                   Converters.ToTimestamp(hookStepProcessor.StepStarted.ToUniversalTime()));
    }

    public virtual TestStepFinished ToTestStepFinished(HookStepTracker hookStepProcessor)
    {
        return new TestStepFinished(hookStepProcessor.TestCaseStartedID,
                                    hookStepProcessor.Definition.TestStepId,
                                    ToTestStepResult(hookStepProcessor), Converters.ToTimestamp(hookStepProcessor.StepFinished.ToUniversalTime()));
    }

    public virtual Attachment ToAttachment(AttachmentAddedEventWrapper tracker)
    {
        var attEvent = tracker.AttachmentAddedEvent;
        return new Attachment(
            Base64EncodeFile(attEvent.FilePath),
            AttachmentContentEncoding.BASE64,
            Path.GetFileName(attEvent.FilePath),
            FileExtensionToMimeTypeMap.GetMimeType(Path.GetExtension(attEvent.FilePath)),
            null,
            tracker.TestCaseStartedId,
            tracker.TestCaseStepId,
            null,
            tracker.TestRunStartedId);
    }

    public virtual Attachment ToAttachment(OutputAddedEventWrapper tracker)
    {
        var outputAddedEvent = tracker.OutputAddedEvent;
        return new Attachment(
            outputAddedEvent.Text,
            AttachmentContentEncoding.IDENTITY,
            null,
            "text/x.cucumber.log+plain",
            null,
            tracker.TestCaseStartedId,
            tracker.TestCaseStepId,
            null,
            tracker.TestRunStartedId);
    }

    private static TestStepResult ToTestStepResult(StepExecutionTrackerBase stepState)
    {
        return new TestStepResult(
            Converters.ToDuration(stepState.Duration),
            "",
            ToTestStepResultStatus(stepState.Status),
            ToException(stepState.Exception)
        );
    }

    private static TestStepResult ToTestStepResult(TestRunHookTracker hookTracker)
    {
        return new TestStepResult(
            Converters.ToDuration(hookTracker.Duration),
            "",
            ToTestStepResultStatus(hookTracker.Status),
            ToException(hookTracker.Exception));
    }

    private static Io.Cucumber.Messages.Types.Exception ToException(System.Exception exception)
    {
        if (exception == null) return null;

        return new Io.Cucumber.Messages.Types.Exception(
            exception.GetType().Name,
            exception.Message,
            exception.StackTrace
        );
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

    public virtual Meta ToMeta(IObjectContainer container)
    {
        var environmentInfoProvider = container.Resolve<IEnvironmentInfoProvider>();
        var environmentWrapper = container.Resolve<IEnvironmentWrapper>();

        var implementation = new Product("Reqnroll", environmentInfoProvider.GetReqnrollVersion());
        string targetFramework = environmentInfoProvider.GetNetCoreVersion() ?? RuntimeInformation.FrameworkDescription;

        var runTime = new Product("dotNet", targetFramework);
        var os = new Product(environmentInfoProvider.GetOSPlatform(), RuntimeInformation.OSDescription);

        var cpu = RuntimeInformation.ProcessArchitecture switch
        {
            Architecture.Arm => new Product("arm", null),
            Architecture.Arm64 => new Product("arm64", null),
            Architecture.X86 => new Product("x86", null),
            Architecture.X64 => new Product("x64", null),
            _ => new Product("unknown", null),
        };

        var ci = ToCi(environmentInfoProvider, environmentWrapper);

        return new Meta(
            ProtocolVersion.Version.Split('+')[0],
            implementation,
            runTime,
            os,
            cpu,
            ci);
    }

    private static Ci ToCi(IEnvironmentInfoProvider environmentInfoProvider, IEnvironmentWrapper environmentWrapper)
    {
        //TODO: Find a way to abstract how various CI systems convey links to builds and build numbers.
        //      Until then, these will be hard coded as null
        var ciName = environmentInfoProvider.GetBuildServerName();
        if (string.IsNullOrEmpty(ciName)) return null;

        var git = ToGit(environmentWrapper);

        return new Ci(ciName, null, null, git);
    }

    private static Git ToGit(IEnvironmentWrapper environmentWrapper)
    {
        Git git;
        var gitUrl = environmentWrapper.GetEnvironmentVariable("GIT_URL");
        var gitBranch = environmentWrapper.GetEnvironmentVariable("GIT_BRANCH");
        var gitCommit = environmentWrapper.GetEnvironmentVariable("GIT_COMMIT");
        var gitTag = environmentWrapper.GetEnvironmentVariable("GIT_TAG");
        if (gitUrl is not ISuccess<string> gitUrlSuccess) git = null;
        else
            git = new Git
            (
                gitUrlSuccess.Result,
                gitBranch is ISuccess<string> branchSuccess ? branchSuccess.Result : null,
                gitCommit is ISuccess<string> commitSuccess ? commitSuccess.Result : null,
                gitTag is ISuccess<string> tagSuccess ? tagSuccess.Result : null
            );
        return git;
    }

    #region utility methods
    public virtual string CanonicalizeStepDefinitionPattern(IStepDefinitionBinding stepDefinition)
    {
        string signature = GenerateSignature(stepDefinition);

        return $"{stepDefinition.Method.Type.AssemblyName}.{stepDefinition.Method.Type.FullName}.{stepDefinition.Method.Name}({signature})";
    }

    public virtual string CanonicalizeHookBinding(IHookBinding hookBinding)
    {
        string signature = GenerateSignature(hookBinding);
        return $"{hookBinding.Method.Type.AssemblyName}.{hookBinding.Method.Type.FullName}.{hookBinding.Method.Name}({signature})";
    }

    private static string GenerateSignature(IBinding stepDefinition)
    {
        return stepDefinition.Method != null ? string.Join(",", stepDefinition.Method.Parameters.Select(p => p.Type.Name)) : "";
    }
    private static string Base64EncodeFile(string filePath)
    {
        byte[] fileBytes = File.ReadAllBytes(filePath);
        return Convert.ToBase64String(fileBytes);
    }

    private static string NormalizePrimitiveTypeNamesToCucumberTypeNames(string name)
    {
        return name switch
        {
            "Int16" => "short",
            "Int32" => "int",
            "Int64" => "long",
            "Single" => "float",
            "Double" => "double",
            "Byte" => "byte",
            "String" => "string",
            "Boolean" => "bool",
            "Decimal" => "decimal",
            "BigInteger" => "biginteger",
            _ => name
        };
    }
    #endregion
}