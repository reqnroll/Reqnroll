using Cucumber.Messages;
using Gherkin.CucumberMessages;
using Io.Cucumber.Messages.Types;
using Reqnroll.Analytics;
using Reqnroll.Bindings;
using Reqnroll.CommonModels;
using Reqnroll.CucumberMessages.PayloadProcessing;
using Reqnroll.CucumberMessages.RuntimeSupport;
using Reqnroll.EnvironmentAccess;
using Reqnroll.Events;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using static System.Net.Mime.MediaTypeNames;

namespace Reqnroll.CucumberMessages.ExecutionTracking
{
    internal class CucumberMessageFactory
    {
        public static TestRunStarted ToTestRunStarted(FeatureStartedEvent featureStartedEvent)
        {
            return new TestRunStarted(Converters.ToTimestamp(featureStartedEvent.Timestamp));
        }

        public static TestRunFinished ToTestRunFinished(bool testRunStatus, FeatureFinishedEvent testRunFinishedEvent)
        {
            return new TestRunFinished(null, testRunStatus, Converters.ToTimestamp(testRunFinishedEvent.Timestamp), null);
        }
        internal static TestCase ToTestCase(TestCaseCucumberMessageTracker testCaseTracker, ScenarioStartedEvent scenarioStartedEvent)
        {
            var testSteps = new List<TestStep>();

            foreach (var stepState in testCaseTracker.Steps)
            {
                switch (stepState)
                {
                    case TestStepTracker _:
                        var testStep = ToPickleTestStep(testCaseTracker, stepState as TestStepTracker);
                        testSteps.Add(testStep);
                        break;
                    case HookStepTracker _:
                        var hookTestStep = ToHookTestStep(stepState as HookStepTracker);
                        testSteps.Add(hookTestStep);
                        break;
                    default:
                        throw new NotImplementedException();
                }
            }
            var testCase = new TestCase
            (
                testCaseTracker.TestCaseId,
                testCaseTracker.PickleId,
                testSteps
            );
            return testCase;
        }
        internal static TestCaseStarted ToTestCaseStarted(TestCaseCucumberMessageTracker testCaseTracker, ScenarioStartedEvent scenarioStartedEvent)
        {
            return new TestCaseStarted(0, testCaseTracker.TestCaseStartedId, testCaseTracker.TestCaseId, null, Converters.ToTimestamp(scenarioStartedEvent.Timestamp));
        }
        internal static TestCaseFinished ToTestCaseFinished(TestCaseCucumberMessageTracker testCaseTracker, ScenarioFinishedEvent scenarioFinishedEvent)
        {
            return new TestCaseFinished(testCaseTracker.TestCaseStartedId, Converters.ToTimestamp(scenarioFinishedEvent.Timestamp), false);
        }
        internal static StepDefinition ToStepDefinition(IStepDefinitionBinding binding, IIdGenerator idGenerator)
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

        internal static StepDefinitionPattern ToStepDefinitionPattern(IStepDefinitionBinding binding)
        {
            var bindingSourceText = binding.SourceExpression;
            var expressionType = binding.ExpressionType;
            var stepDefinitionPatternType = expressionType switch { StepDefinitionExpressionTypes.CucumberExpression => StepDefinitionPatternType.CUCUMBER_EXPRESSION, _ => StepDefinitionPatternType.REGULAR_EXPRESSION };
            var stepDefinitionPattern = new StepDefinitionPattern(bindingSourceText, stepDefinitionPatternType);
            return stepDefinitionPattern;
        }
        internal static UndefinedParameterType ToUndefinedParameterType(string expression, string paramName, IIdGenerator iDGenerator)
        {
            return new UndefinedParameterType(expression, paramName);
        }

        internal static ParameterType ToParameterType(IStepArgumentTransformationBinding stepTransform, IIdGenerator iDGenerator)
        {
            var regex = stepTransform.Regex;
            var regexPattern = regex == null ? null : regex.ToString();
            var name = stepTransform.Name ?? stepTransform.Method.ReturnType.Name;
            var result = new ParameterType
            (
                name,
                new List<string>
                {
                    regexPattern
                },
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
            var className = binding.Method.Type.Name;
            var paramTypes = binding.Method.Parameters.Select(x => x.Type.Name).ToList();
            var methodDescription = new JavaMethod(className, methodName, paramTypes);
            var sourceRef = SourceReference.Create(methodDescription);
            return sourceRef;
        }

        internal static TestStep ToPickleTestStep(TestCaseCucumberMessageTracker tracker, TestStepTracker stepState)
        {
            bool bound = stepState.Bound;
            bool ambiguous = stepState.Ambiguous;

            var args = stepState.StepArguments
               .Select(arg => ToStepMatchArgument(arg))
               .ToList();

            var result = new TestStep(
                 null,
                 stepState.TestStepID,
                 stepState.PickleStepID,
                 bound ? new List<string> { stepState.StepDefinitionId } : ambiguous ? new List<string>(stepState.AmbiguousStepDefinitions) : new List<string>(),
                 bound ? new List<StepMatchArgumentsList> { new StepMatchArgumentsList(args) } : new List<StepMatchArgumentsList>()
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
                NormalizePrimitiveTypeNamesToCucumberTypeNames(argument.Type));
        }
        internal static TestStepStarted ToTestStepStarted(TestStepTracker stepState, StepStartedEvent stepStartedEvent)
        {
            return new TestStepStarted(
                stepState.TestCaseStartedID,
                stepState.TestStepID,
                Converters.ToTimestamp(stepStartedEvent.Timestamp));
        }

        internal static TestStepFinished ToTestStepFinished(TestStepTracker stepState, StepFinishedEvent stepFinishedEvent)
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
                hookBinding.IsScoped ? $"@{hookBinding.BindingScope.Tag}" : null
            );
            return result;
        }

        internal static TestStep ToHookTestStep(HookStepTracker hookStepState)
        {
            // find the Hook message at the Feature level
            var hookCacheKey = CanonicalizeHookBinding(hookStepState.HookBindingFinishedEvent.HookBinding);
            var hookId = hookStepState.ParentTestCase.StepDefinitionsByPattern[hookCacheKey];

            return new TestStep(
                hookId,
                hookStepState.TestStepID,
                null,
                null,
                null);
        }
        internal static TestStepStarted ToTestStepStarted(HookStepTracker hookStepProcessor, HookBindingStartedEvent hookBindingStartedEvent)
        {
            return new TestStepStarted(hookStepProcessor.TestCaseStartedID, hookStepProcessor.TestStepID, Converters.ToTimestamp(hookBindingStartedEvent.Timestamp));
        }

        internal static TestStepFinished ToTestStepFinished(HookStepTracker hookStepProcessor, HookBindingFinishedEvent hookFinishedEvent)
        {
            return new TestStepFinished(hookStepProcessor.TestCaseStartedID, hookStepProcessor.TestStepID, ToTestStepResult(hookStepProcessor), Converters.ToTimestamp(hookFinishedEvent.Timestamp));
        }

        internal static Attachment ToAttachment(TestCaseCucumberMessageTracker tracker, AttachmentAddedEventWrapper attachmentAddedEventWrapper)
        {
            return new Attachment(
                Base64EncodeFile(attachmentAddedEventWrapper.AttachmentAddedEvent.FilePath),
                AttachmentContentEncoding.BASE64,
                Path.GetFileName(attachmentAddedEventWrapper.AttachmentAddedEvent.FilePath),
                FileExtensionToMIMETypeMap.GetMimeType(Path.GetExtension(attachmentAddedEventWrapper.AttachmentAddedEvent.FilePath)),
                null,
                attachmentAddedEventWrapper.TestCaseStartedID,
                attachmentAddedEventWrapper.TestCaseStepID,
                null);
        }
        internal static Attachment ToAttachment(TestCaseCucumberMessageTracker tracker, OutputAddedEventWrapper outputAddedEventWrapper)
        {
            return new Attachment(
                outputAddedEventWrapper.OutputAddedEvent.Text,
                AttachmentContentEncoding.IDENTITY,
                null,
                "text/x.cucumber.log+plain",
                null,
                outputAddedEventWrapper.TestCaseStartedID,
                outputAddedEventWrapper.TestCaseStepID,
                null);
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

        public static Envelope ToMeta(FeatureStartedEvent featureStartedEvent)
        {
            var featureContainer = featureStartedEvent.FeatureContext.FeatureContainer;
            var environmentInfoProvider = featureContainer.Resolve<IEnvironmentInfoProvider>();
            var environmentWrapper = featureContainer.Resolve<IEnvironmentWrapper>();

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
                _ => new Product(null, null),
            };

            var ci_name = environmentInfoProvider.GetBuildServerName();

            var ci = ToCi(ci_name, environmentInfoProvider, environmentWrapper);

            return Envelope.Create(new Meta(
                    ProtocolVersion.Version.Split('+')[0],
                    implementation,
                    runTime,
                    os,
                    cpu,
                    ci));
        }

        private static Ci ToCi(string ci_name, IEnvironmentInfoProvider environmentInfoProvider, IEnvironmentWrapper environmentWrapper)
        {
            //TODO: Find a way to abstract how various CI systems convey links to builds and build numbers.
            //      Until then, these will be hard coded as null
            if (string.IsNullOrEmpty(ci_name)) return null;

            var git = ToGit(environmentWrapper);

            return new Ci(ci_name, null, null, git);
        }

        private static Git ToGit(IEnvironmentWrapper environmentWrapper)
        {
            Git git;
            var git_url = environmentWrapper.GetEnvironmentVariable("GIT_URL");
            var git_branch = environmentWrapper.GetEnvironmentVariable("GIT_BRANCH");
            var git_commit = environmentWrapper.GetEnvironmentVariable("GIT_COMMIT");
            var git_tag = environmentWrapper.GetEnvironmentVariable("GIT_TAG");
            if (git_url is not ISuccess<string>) git = null;
            else
                git = new Git
                (
                    (git_url as ISuccess<string>).Result,
                    git_branch is ISuccess<string> ? (git_branch as ISuccess<string>).Result : null,
                    git_commit is ISuccess<string> ? (git_commit as ISuccess<string>).Result : null,
                    git_tag is ISuccess<string> ? (git_tag as ISuccess<string>).Result : null
                );
            return git;
        }

        #region utility methods
        public static string CanonicalizeStepDefinitionPattern(IStepDefinitionBinding stepDefinition)
        {
            string signature = GenerateSignature(stepDefinition);

            return $"{stepDefinition.SourceExpression}({signature})";
        }

        public static string CanonicalizeHookBinding(IHookBinding hookBinding)
        {
            string signature = GenerateSignature(hookBinding);
            return $"{hookBinding.Method.Type.FullName}.{hookBinding.Method.Name}({signature})";
        }

        private static string GenerateSignature(IBinding stepDefinition)
        {
            return stepDefinition.Method != null ? string.Join(",", stepDefinition.Method.Parameters.Select(p => p.Type.Name)) : "";
        }
        public static string Base64EncodeFile(string filePath)
        {
            if (Path.GetExtension(filePath) == ".png" || Path.GetExtension(filePath) == ".jpg")
            {
                byte[] fileBytes = File.ReadAllBytes(filePath);
                return Convert.ToBase64String(fileBytes);
            }
            // else assume its a text file
            string text = File.ReadAllText(filePath);
            text = text.Replace("\r\n", "\n");
            return Convert.ToBase64String(Encoding.UTF8.GetBytes(text));
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
}