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
public class CucumberMessageFactory : ICucumberMessageFactory
{
    public virtual TestRunStarted ToTestRunStarted(DateTime timestamp, string id)
    {
        return new TestRunStarted(Converters.ToTimestamp(timestamp.ToUniversalTime()), id);
    }

    public virtual TestRunFinished ToTestRunFinished(bool testRunStatus, DateTime timestamp, string testRunStartedId)
    {
        return new TestRunFinished(null, testRunStatus, Converters.ToTimestamp(timestamp.ToUniversalTime()), null, testRunStartedId);
    }

    public virtual TestRunHookStarted ToTestRunHookStarted(TestRunHookExecutionTracker hookExecutionTracker)
    {
        return new TestRunHookStarted(hookExecutionTracker.HookStartedId, hookExecutionTracker.TestRunId, hookExecutionTracker.HookId, Converters.ToTimestamp(hookExecutionTracker.HookStarted?.ToUniversalTime() ?? DateTime.MinValue));
    }

    public virtual TestRunHookFinished ToTestRunHookFinished(TestRunHookExecutionTracker hookExecutionTracker)
    {
        return new TestRunHookFinished(hookExecutionTracker.HookStartedId, ToTestStepResult(hookExecutionTracker), Converters.ToTimestamp(hookExecutionTracker.HookFinished?.ToUniversalTime() ?? DateTime.MinValue));
    }

    public virtual TestCaseStarted ToTestCaseStarted(TestCaseExecutionTracker testCaseExecution, string testCaseId)
    {
        return new TestCaseStarted(
            testCaseExecution.AttemptId,
            testCaseExecution.TestCaseStartedId,
            testCaseId,
            null,
            Converters.ToTimestamp(testCaseExecution.TestCaseStartedTimeStamp.ToUniversalTime()));
    }

    public virtual TestCaseFinished ToTestCaseFinished(TestCaseExecutionTracker testCaseExecution)
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
        var stepDefinitionPatternType = binding.ExpressionType switch
        {
            StepDefinitionExpressionTypes.CucumberExpression => StepDefinitionPatternType.CUCUMBER_EXPRESSION,
            _ => StepDefinitionPatternType.REGULAR_EXPRESSION
        };
        return new StepDefinitionPattern(binding.SourceExpression, stepDefinitionPatternType);
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

    public virtual TestCase ToTestCase(TestCaseTracker testCaseTracker)
    {
        var testSteps = new List<TestStep>();

        foreach (var stepTracker in testCaseTracker.Steps)
        {
            switch (stepTracker)
            {
                case HookStepTracker hookStepDefinition:
                    var hookTestStep = ToTestStep(hookStepDefinition);
                    testSteps.Add(hookTestStep);
                    break;
                case TestStepTracker testStepDefinition:
                    var testStep = ToTestStep(testStepDefinition);
                    testSteps.Add(testStep);
                    break;
                default:
                    throw new NotImplementedException();
            }
        }
        var testCase = new TestCase
        (
            testCaseTracker.TestCaseId,
            testCaseTracker.PickleId,
            testSteps,
            testCaseTracker.ParentTracker.TestRunStartedId
        );
        return testCase;
    }

    public virtual TestStep ToTestStep(TestStepTracker stepDef)
    {
        var args = stepDef.StepArguments
                          .Select(ToStepMatchArgument)
                          .ToList();

        var result = new TestStep(
            null,
            stepDef.TestStepId,
            stepDef.PickleStepId,
            stepDef.StepDefinitionIds,
            stepDef.IsBound ? [new StepMatchArgumentsList(args)] : []
        );

        return result;
    }

    public virtual TestStep ToTestStep(HookStepTracker hookStepTracker)
    {
        var hookId = hookStepTracker.HookId;

        return new TestStep(
            hookId,
            hookStepTracker.TestStepId,
            null,
            null,
            null);
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

    public virtual TestStepStarted ToTestStepStarted(TestStepExecutionTracker testStepExecutionTracker)
    {
        DateTime ts = testStepExecutionTracker.StepStartedAt.HasValue ? (DateTime) testStepExecutionTracker.StepStartedAt! : new DateTime();
        return new TestStepStarted(
            testStepExecutionTracker.TestCaseStartedId,
            testStepExecutionTracker.StepTracker.TestStepId,
            Converters.ToTimestamp(ts.ToUniversalTime()));
    }

    public virtual TestStepFinished ToTestStepFinished(TestStepExecutionTracker testStepExecutionTracker)
    {
        return new TestStepFinished(
            testStepExecutionTracker.TestCaseStartedId,
            testStepExecutionTracker.StepTracker.TestStepId,
            ToTestStepResult(testStepExecutionTracker),
            Converters.ToTimestamp(((DateTime)testStepExecutionTracker.StepFinishedAt).ToUniversalTime()));
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

    public virtual TestStepStarted ToTestStepStarted(HookStepExecutionTracker hookStepExecutionTracker)
    {
        return new TestStepStarted(hookStepExecutionTracker.TestCaseStartedId,
                                   hookStepExecutionTracker.StepTracker.TestStepId,
                                   Converters.ToTimestamp(((DateTime)hookStepExecutionTracker.StepStartedAt).ToUniversalTime()));
    }

    public virtual TestStepFinished ToTestStepFinished(HookStepExecutionTracker hookStepExecutionTracker)
    {
        return new TestStepFinished(hookStepExecutionTracker.TestCaseStartedId,
                                    hookStepExecutionTracker.StepTracker.TestStepId,
                                    ToTestStepResult(hookStepExecutionTracker), Converters.ToTimestamp(((DateTime)hookStepExecutionTracker.StepFinishedAt).ToUniversalTime()));
    }

    public virtual Attachment ToAttachment(AttachmentTracker tracker)
    {
        var filePath = tracker.FilePath;
        return new Attachment(
            Base64EncodeFile(filePath),
            AttachmentContentEncoding.BASE64,
            Path.GetFileName(filePath),
            FileExtensionToMimeTypeMap.GetMimeType(Path.GetExtension(filePath)),
            null, //Source
            tracker.TestCaseStartedId,
            tracker.TestCaseStepId,
            null, //url
            tracker.TestRunStartedId,
            tracker.TestRunHookStartedId,
            Converters.ToTimestamp(tracker.Timestamp));
    }

    public virtual Attachment ToAttachment(OutputMessageTracker tracker)
    {
        return new Attachment(
            tracker.Text,
            AttachmentContentEncoding.IDENTITY,
            null,
            "text/x.cucumber.log+plain",
            null,
            tracker.TestCaseStartedId,
            tracker.TestCaseStepId,
            null,
            tracker.TestRunStartedId,
            tracker.TestRunHookStartedId,
            Converters.ToTimestamp(tracker.Timestamp));
    }

    private static TestStepResult ToTestStepResult(StepExecutionTrackerBase stepState)
    {
        TimeSpan d = (stepState.Duration.HasValue ? (TimeSpan) stepState.Duration : new TimeSpan(0));
        return new TestStepResult(
            Converters.ToDuration(d),
            "",
            ToTestStepResultStatus(stepState.Status),
            ToException(stepState.Exception)
        );
    }

    private static TestStepResult ToTestStepResult(TestRunHookExecutionTracker hookExecutionTracker)
    {
        return new TestStepResult(
            Converters.ToDuration(hookExecutionTracker.Duration ?? TimeSpan.Zero),
            "",
            ToTestStepResultStatus(hookExecutionTracker.Status),
            ToException(hookExecutionTracker.Exception));
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