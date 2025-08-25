using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Gherkin.CucumberMessages;
using Reqnroll.Configuration;
using Reqnroll.Formatters.RuntimeSupport;
using Reqnroll.Generator.CodeDom;
using Reqnroll.Generator.UnitTestConverter;
using Reqnroll.Generator.UnitTestProvider;
using Reqnroll.Parser;
using Reqnroll.Parser.CucumberMessages;
using Reqnroll.Tracing;


namespace Reqnroll.Generator.Generation
{
    [SuppressMessage("ReSharper", "BitwiseOperatorOnEnumWithoutFlags")]
    public class UnitTestFeatureGenerator : IFeatureGenerator
    {
        private readonly CodeDomHelper _codeDomHelper;
        private readonly IDecoratorRegistry _decoratorRegistry;
        private readonly ScenarioPartHelper _scenarioPartHelper;
        private readonly ReqnrollConfiguration _reqnrollConfiguration;
        private readonly IUnitTestGeneratorProvider _testGeneratorProvider;
        private readonly UnitTestMethodGenerator _unitTestMethodGenerator;
        private readonly LinePragmaHandler _linePragmaHandler;
        private CodeMemberMethod _cucumberMessagesInitializeMethod;

        public UnitTestFeatureGenerator(
            IUnitTestGeneratorProvider testGeneratorProvider,
            CodeDomHelper codeDomHelper,
            ReqnrollConfiguration reqnrollConfiguration,
            IDecoratorRegistry decoratorRegistry)
        {
            _testGeneratorProvider = testGeneratorProvider;
            _codeDomHelper = codeDomHelper;
            _reqnrollConfiguration = reqnrollConfiguration;
            _decoratorRegistry = decoratorRegistry;
            _linePragmaHandler = new LinePragmaHandler(_reqnrollConfiguration, _codeDomHelper);
            _scenarioPartHelper = new ScenarioPartHelper(_reqnrollConfiguration, _codeDomHelper);
            _unitTestMethodGenerator = new UnitTestMethodGenerator(testGeneratorProvider, decoratorRegistry, _codeDomHelper, _scenarioPartHelper, _reqnrollConfiguration);
        }

        public string TestClassNameFormat { get; set; } = "{0}Feature";

        public CodeNamespace GenerateUnitTestFixture(ReqnrollDocument document, string testClassName, string targetNamespace, out IEnumerable<string> generationWarnings, bool featureFilesEmbedded = false)
        {
            var codeNamespace = CreateNamespace(targetNamespace);
            var feature = document.ReqnrollFeature;

            testClassName = testClassName ?? string.Format(TestClassNameFormat, feature.Name.ToIdentifier());
            var generationContext = CreateTestClassStructure(codeNamespace, testClassName, document, featureFilesEmbedded);

            SetupTestClass(generationContext);
            SetupTestClassInitializeMethod(generationContext);
            SetupTestClassCleanupMethod(generationContext);

            SetupScenarioStartMethod(generationContext);
            SetupScenarioInitializeMethod(generationContext);
            _scenarioPartHelper.SetupFeatureBackground(generationContext);
            SetupScenarioCleanupMethod(generationContext);

            SetupTestInitializeMethod(generationContext);
            SetupTestCleanupMethod(generationContext);

            _unitTestMethodGenerator.CreateUnitTests(feature, generationContext);

            //before returning the generated code, call the provider's method in case the generated code needs to be customized            
            _testGeneratorProvider.FinalizeTestClass(generationContext);

            generationWarnings = generationContext.GenerationWarnings;
            return codeNamespace;
        }


        private TestClassGenerationContext CreateTestClassStructure(CodeNamespace codeNamespace, string testClassName, ReqnrollDocument document, bool featureFilesEmbedded)
        {
            var testClass = _codeDomHelper.CreateGeneratedTypeDeclaration(testClassName);
            codeNamespace.Types.Add(testClass);

            return new TestClassGenerationContext(
                _testGeneratorProvider,
                document,
                codeNamespace,
                testClass,
                DeclareTestRunnerMember(testClass),
                _codeDomHelper.CreateMethod(testClass),
                _codeDomHelper.CreateMethod(testClass),
                _codeDomHelper.CreateMethod(testClass),
                _codeDomHelper.CreateMethod(testClass),
                _codeDomHelper.CreateMethod(testClass),
                _codeDomHelper.CreateMethod(testClass),
                _codeDomHelper.CreateMethod(testClass),
                document.ReqnrollFeature.HasFeatureBackground() ? _codeDomHelper.CreateMethod(testClass) : null,
                _testGeneratorProvider.GetTraits().HasFlag(UnitTestGeneratorTraits.RowTests) && _reqnrollConfiguration.AllowRowTests,
                featureFilesEmbedded);
        }

        private CodeNamespace CreateNamespace(string targetNamespace)
        {
            targetNamespace = targetNamespace ?? GeneratorConstants.DEFAULT_NAMESPACE;

            if (!targetNamespace.StartsWith("global", StringComparison.CurrentCultureIgnoreCase))
            {
                switch (_codeDomHelper.TargetLanguage)
                {
                    case CodeDomProviderLanguage.VB:
                        targetNamespace = $"GlobalVBNetNamespace.{targetNamespace}";
                        break;
                }
            }

            var codeNamespace = new CodeNamespace(targetNamespace);

            return codeNamespace;
        }

        private void SetupScenarioCleanupMethod(TestClassGenerationContext generationContext)
        {
            var scenarioCleanupMethod = generationContext.ScenarioCleanupMethod;

            scenarioCleanupMethod.Attributes = MemberAttributes.Public | MemberAttributes.Final;
            scenarioCleanupMethod.Name = GeneratorConstants.SCENARIO_CLEANUP_NAME;

            _codeDomHelper.MarkCodeMemberMethodAsAsync(scenarioCleanupMethod);

            // call collect errors
            var testRunnerField = _scenarioPartHelper.GetTestRunnerExpression();

            //await testRunner.CollectScenarioErrorsAsync();
            var expression = new CodeMethodInvokeExpression(
                testRunnerField,
                nameof(ITestRunner.CollectScenarioErrorsAsync));

            _codeDomHelper.MarkCodeMethodInvokeExpressionAsAwait(expression);

            scenarioCleanupMethod.Statements.Add(expression);
        }

        private void SetupTestClass(TestClassGenerationContext generationContext)
        {
            generationContext.TestClass.IsPartial = true;
            generationContext.TestClass.TypeAttributes |= TypeAttributes.Public;

            _linePragmaHandler.AddLinePragmaInitial(generationContext.TestClass, generationContext.Document.SourceFilePath);

            _testGeneratorProvider.SetTestClass(generationContext, generationContext.Feature.Name, generationContext.Feature.Description);

            _decoratorRegistry.DecorateTestClass(generationContext, out var featureCategories);

            if (featureCategories.Any())
            {
                _testGeneratorProvider.SetTestClassCategories(generationContext, featureCategories);
            }

            DeclareFeatureTagsField(generationContext);
            DeclareFeatureMessagesFactoryMembers(generationContext);
            DeclareFeatureInfoMember(generationContext);
        }

        private CodeMemberField DeclareTestRunnerMember(CodeTypeDeclaration type)
        {
            var testRunnerField = new CodeMemberField(new CodeTypeReference(typeof(ITestRunner), CodeTypeReferenceOptions.GlobalReference), GeneratorConstants.TESTRUNNER_FIELD);
            type.Members.Add(testRunnerField);
            return testRunnerField;
        }

        private void DeclareFeatureTagsField(TestClassGenerationContext generationContext)
        {
            var featureTagsField = new CodeMemberField(typeof(string[]), GeneratorConstants.FEATURE_TAGS_VARIABLE_NAME);
            featureTagsField.Attributes |= MemberAttributes.Static;
            featureTagsField.InitExpression = _scenarioPartHelper.GetStringArrayExpression(generationContext.Feature.Tags);
            generationContext.TestClass.Members.Add(featureTagsField);
        }

        private void DeclareFeatureInfoMember(TestClassGenerationContext generationContext)
        {
            var featureInfoField = new CodeMemberField(
                new CodeTypeReference(typeof(FeatureInfo), CodeTypeReferenceOptions.GlobalReference), GeneratorConstants.FEATUREINFO_FIELD);
            featureInfoField.Attributes |= MemberAttributes.Static;
            featureInfoField.InitExpression = new CodeObjectCreateExpression(new CodeTypeReference(typeof(FeatureInfo), CodeTypeReferenceOptions.GlobalReference),
                new CodeObjectCreateExpression(new CodeTypeReference(typeof(CultureInfo), CodeTypeReferenceOptions.GlobalReference),
                                               new CodePrimitiveExpression(generationContext.Feature.Language)),
                new CodePrimitiveExpression(generationContext.Document.DocumentLocation?.FeatureFolderPath),
                new CodePrimitiveExpression(generationContext.Feature.Name),
                new CodePrimitiveExpression(generationContext.Feature.Description),
                new CodeFieldReferenceExpression(
                    new CodeTypeReferenceExpression(new CodeTypeReference(typeof(ProgrammingLanguage), CodeTypeReferenceOptions.GlobalReference)),
                    _codeDomHelper.TargetLanguage.ToString()),
                new CodeFieldReferenceExpression(null, GeneratorConstants.FEATURE_TAGS_VARIABLE_NAME),
                new CodeMethodInvokeExpression(null, _cucumberMessagesInitializeMethod.Name));

            generationContext.TestClass.Members.Add(featureInfoField);
        }

        private void DeclareFeatureMessagesFactoryMembers(TestClassGenerationContext generationContext)
        {
            // Generation of Cucumber Messages relies on access to the parsed AST from 'generationContext.Document'

            var sourceFileLocation = Path.Combine(generationContext.Document.DocumentLocation.FeatureFolderPath, generationContext.Document.DocumentLocation.SourceFilePath);

            // Adding three static methods to the test class: one each as factory methods for source, gherkinDocument, and pickles Messages
            // Bodies of these methods are added later inside the try/catch block
            var sourceFunc = new CodeDelegateCreateExpression(new CodeTypeReference(typeof(Func<Io.Cucumber.Messages.Types.Source>), CodeTypeReferenceOptions.GlobalReference), new CodeTypeReferenceExpression(generationContext.TestClass.Name), "SourceFunc");
            var sourceFactoryMethod = new CodeMemberMethod
            {
                Attributes = MemberAttributes.Private | MemberAttributes.Static,
                ReturnType = new CodeTypeReference(typeof(Io.Cucumber.Messages.Types.Source), CodeTypeReferenceOptions.GlobalReference),
                Name = sourceFunc.MethodName
            };
            generationContext.TestClass.Members.Add(sourceFactoryMethod);

            var gherkinDocumentFunc = new CodeDelegateCreateExpression(new CodeTypeReference(typeof(Func<Io.Cucumber.Messages.Types.GherkinDocument>), CodeTypeReferenceOptions.GlobalReference), new CodeTypeReferenceExpression(generationContext.TestClass.Name), "GherkinDocumentFunc");
            var gherkinDocumentFactoryMethod = new CodeMemberMethod
            {
                Attributes = MemberAttributes.Private | MemberAttributes.Static,
                ReturnType = new CodeTypeReference(typeof(Io.Cucumber.Messages.Types.GherkinDocument), CodeTypeReferenceOptions.GlobalReference),
                Name = gherkinDocumentFunc.MethodName
            };
            generationContext.TestClass.Members.Add(gherkinDocumentFactoryMethod);

            var picklesFunc = new CodeDelegateCreateExpression(new CodeTypeReference(typeof(Func<IEnumerable<Io.Cucumber.Messages.Types.Pickle>>), CodeTypeReferenceOptions.GlobalReference), new CodeTypeReferenceExpression(generationContext.TestClass.Name), "PicklesFunc");
            var picklesFactoryMethod = new CodeMemberMethod
            {
                Attributes = MemberAttributes.Private | MemberAttributes.Static,
                ReturnType = new CodeTypeReference(typeof(IEnumerable<Io.Cucumber.Messages.Types.Pickle>), CodeTypeReferenceOptions.GlobalReference),
                Name = picklesFunc.MethodName
            };
            generationContext.TestClass.Members.Add(picklesFactoryMethod);

            // Create a new method that will be added to the test class.
            // It will be called to provide the FeatureCucumberMessages property value of the FeatureInfo object when that object is constructed
            var cucumberMessagesInitializeMethod = new CodeMemberMethod
            {
                Attributes = MemberAttributes.Private | MemberAttributes.Static,
                Name = "InitializeCucumberMessages",
                ReturnType = new CodeTypeReference(typeof(FeatureLevelCucumberMessages), CodeTypeReferenceOptions.GlobalReference)
            };
            generationContext.TestClass.Members.Add(cucumberMessagesInitializeMethod);
            _cucumberMessagesInitializeMethod = cucumberMessagesInitializeMethod;

            // Create a FeatureLevelCucumberMessages object and add it to featureInfo
            var featureLevelCucumberMessagesExpression = new CodeObjectCreateExpression(new CodeTypeReference(typeof(FeatureLevelCucumberMessages), CodeTypeReferenceOptions.GlobalReference),
                sourceFunc,
                gherkinDocumentFunc,
                picklesFunc);

            cucumberMessagesInitializeMethod.Statements.Add(
                new CodeMethodReturnStatement(
                    featureLevelCucumberMessagesExpression));

            var sourceReturnStatement = new CodeMethodReturnStatement(new CodePrimitiveExpression(null));
            var gherkinDocumentReturnStatement = new CodeMethodReturnStatement(new CodePrimitiveExpression(null));
            var picklesReturnStatement = new CodeMethodReturnStatement(new CodePrimitiveExpression(null));
            try
            {
                var featureSource = CucumberMessagesConverter.ConvertToCucumberMessagesSource(generationContext.Document);
                if (generationContext.FeatureFilesEmbedded)
                {
                    featureSource = new Io.Cucumber.Messages.Types.Source(featureSource.Uri, "", Io.Cucumber.Messages.Types.SourceMediaType.TEXT_X_CUCUMBER_GHERKIN_PLAIN);
                }
                var IdGeneratorSeed = featureSource.Uri + featureSource.Data;
                var messageConverter = new CucumberMessagesConverter(new DeterministicIdGenerator(IdGeneratorSeed));
                var featureGherkinDocumentMessage = messageConverter.ConvertToCucumberMessagesGherkinDocument(generationContext.Document);
                var featurePickleMessages = messageConverter.ConvertToCucumberMessagesPickles(featureGherkinDocumentMessage);

                // generate a CodeDom expression to create the Source object from the featureSourceMessage
                var sourceExpression = new CodeObjectCreateExpression(new CodeTypeReference(typeof(Io.Cucumber.Messages.Types.Source), CodeTypeReferenceOptions.GlobalReference),
                    new CodePrimitiveExpression(featureSource.Uri),
                    new CodePrimitiveExpression(featureSource.Data),
                    new CodeFieldReferenceExpression(new CodeTypeReferenceExpression(new CodeTypeReference(typeof(Io.Cucumber.Messages.Types.SourceMediaType), CodeTypeReferenceOptions.GlobalReference)), featureSource.MediaType.ToString()));

                // generate a CodeDom expression to create the GherkinDocument object from the featureGherkinDocumentMessage
                var gherkinDocumentExpressionGenerator = new CucumberGherkinDocumentExpressionGenerator();
                var gherkinDocumentExpression = gherkinDocumentExpressionGenerator.GenerateGherkinDocumentExpression(featureGherkinDocumentMessage);

                // generate a CodeDom expression to create the Pickles object from the featurePickleMessages
                var pickleExpressionGenerator = new CucumberPicklesExpressionGenerator();
                var picklesExpression = pickleExpressionGenerator.GeneratePicklesExpression(featurePickleMessages);

                // wrap these expressions in Func<T>

                sourceReturnStatement = new CodeMethodReturnStatement(sourceExpression);

                gherkinDocumentReturnStatement = new CodeMethodReturnStatement(gherkinDocumentExpression);

                picklesReturnStatement = new CodeMethodReturnStatement(picklesExpression);
            }
            catch (Exception e)
            {
                generationContext.GenerationWarnings.Add($"WARNING: Failed to process Cucumber Pickles. Support for generating Cucumber Messages will be disabled. Exception: {e.Message}");
                // Should any error occur during pickling or serialization of Cucumber Messages, we will abort and not add the Cucumber Messages to the featureInfo.
                // This effectively turns OFF the Cucumber Messages support for this feature.
            }
            finally
            {
                sourceFactoryMethod.Statements.Add(sourceReturnStatement);

                gherkinDocumentFactoryMethod.Statements.Add(gherkinDocumentReturnStatement);

                picklesFactoryMethod.Statements.Add(picklesReturnStatement);
            }
        }

        private void SetupTestClassInitializeMethod(TestClassGenerationContext generationContext)
        {
            var testClassInitializeMethod = generationContext.TestClassInitializeMethod;

            testClassInitializeMethod.Attributes = MemberAttributes.Public;
            testClassInitializeMethod.Name = GeneratorConstants.TESTCLASS_INITIALIZE_NAME;

            _codeDomHelper.MarkCodeMemberMethodAsAsync(testClassInitializeMethod);

            _testGeneratorProvider.SetTestClassInitializeMethod(generationContext);
        }

        private void SetupTestClassCleanupMethod(TestClassGenerationContext generationContext)
        {
            var testClassCleanupMethod = generationContext.TestClassCleanupMethod;

            testClassCleanupMethod.Attributes = MemberAttributes.Public;
            testClassCleanupMethod.Name = GeneratorConstants.TESTCLASS_CLEANUP_NAME;

            // Make sure that OnFeatureEndAsync is called on all associated TestRunners.
            // await global::Reqnroll.TestRunnerManager.ReleaseFeatureAsync(featureInfo);
            var releaseFeature = new CodeMethodInvokeExpression(
                    new CodeTypeReferenceExpression(new CodeTypeReference(typeof(TestRunnerManager), CodeTypeReferenceOptions.GlobalReference)),
                    nameof(TestRunnerManager.ReleaseFeatureAsync),
                    new CodeVariableReferenceExpression(GeneratorConstants.FEATUREINFO_FIELD));

            _codeDomHelper.MarkCodeMethodInvokeExpressionAsAwait(releaseFeature);

            testClassCleanupMethod.Statements.Add(releaseFeature);

            _codeDomHelper.MarkCodeMemberMethodAsAsync(testClassCleanupMethod);

            _testGeneratorProvider.SetTestClassCleanupMethod(generationContext);
        }

        private void SetupTestInitializeMethod(TestClassGenerationContext generationContext)
        {
            var testInitializeMethod = generationContext.TestInitializeMethod;

            testInitializeMethod.Attributes = MemberAttributes.Public | MemberAttributes.Final;
            testInitializeMethod.Name = GeneratorConstants.TEST_INITIALIZE_NAME;

            _codeDomHelper.MarkCodeMemberMethodAsAsync(testInitializeMethod);

            _testGeneratorProvider.SetTestInitializeMethod(generationContext);

            // Obtain the test runner for executing a single test
            // testRunner = global::Reqnroll.TestRunnerManager.GetTestRunnerForAssembly(featureHint: featureInfo);

            var testRunnerField = _scenarioPartHelper.GetTestRunnerExpression();

            var getTestRunnerExpression = new CodeMethodInvokeExpression(
                new CodeTypeReferenceExpression(new CodeTypeReference(typeof(TestRunnerManager), CodeTypeReferenceOptions.GlobalReference)),
                nameof(TestRunnerManager.GetTestRunnerForAssembly),
                _codeDomHelper.CreateOptionalArgumentExpression("featureHint",
                    new CodeVariableReferenceExpression(GeneratorConstants.FEATUREINFO_FIELD)));

            testInitializeMethod.Statements.Add(
                new CodeAssignStatement(
                    testRunnerField,
                    getTestRunnerExpression));


            // "Finish" current feature if needed

            var featureContextExpression = new CodePropertyReferenceExpression(
                testRunnerField,
                "FeatureContext");

            var onFeatureEndAsyncExpression = new CodeMethodInvokeExpression(
                testRunnerField,
                nameof(ITestRunner.OnFeatureEndAsync));
            _codeDomHelper.MarkCodeMethodInvokeExpressionAsAwait(onFeatureEndAsyncExpression);

            // VB does not allow the use of the await keyword in a "finally" clause. Therefore, we have to generate code specific to
            // the language. The C# code will await OnFeatureStartAsync() while VB will call the method synchronously and store the returned Task in a variable.
            // The VB code will then call await on that task after the conclusion of the try/finally block.
            // This construct in VB might not fully wait for a real async execution of a before feature hook in case there was an exception in the previous after feature hook,
            // but this affects only very specific and rare cases and only apply for legacy VB usages.

            // Dim onFeatureStartTask as Task = Nothing
            if (_codeDomHelper.TargetLanguage == CodeDomProviderLanguage.VB)
            {
                testInitializeMethod.Statements.Add(new CodeVariableDeclarationStatement(new CodeTypeReference(typeof(Task)), "onFeatureStartTask", new CodePrimitiveExpression(null)));
            }
            // try {
            //   if (testRunner.FeatureContext != null && !testRunner.FeatureContext.FeatureInfo.Equals(featureInfo))
            //     await testRunner.OnFeatureEndAsync(); // finish if different
            // } 
            var conditionallyExecuteOnFeatureEndExpressionStatement =
                new CodeConditionStatement(
                    new CodeBinaryOperatorExpression(
                        new CodeBinaryOperatorExpression(
                            featureContextExpression,
                            CodeBinaryOperatorType.IdentityInequality,
                            new CodePrimitiveExpression(null)),
                        CodeBinaryOperatorType.BooleanAnd,
                        new CodeBinaryOperatorExpression(
                            new CodeMethodInvokeExpression(
                                new CodePropertyReferenceExpression(
                                    featureContextExpression,
                                    "FeatureInfo"),
                                nameof(object.Equals),
                                new CodeVariableReferenceExpression(GeneratorConstants.FEATUREINFO_FIELD)),
                            CodeBinaryOperatorType.ValueEquality,
                            new CodePrimitiveExpression(false))),
                    new CodeExpressionStatement(
                        onFeatureEndAsyncExpression));

            // The following statement will be added to the finally block, to skip 
            // scenario execution of features with a failing before feature hook:
            // if (testRunner.FeatureContext?.BeforeFeatureHookFailed)
            //   throw new ReqnrollException("[before feature hook error]");
            var throwErrorOnPreviousFeatureStartError =
                new CodeConditionStatement(
                    new CodeBinaryOperatorExpression(
                        new CodeBinaryOperatorExpression(
                            featureContextExpression, 
                            CodeBinaryOperatorType.IdentityInequality,
                            new CodePrimitiveExpression(null)),
                        CodeBinaryOperatorType.BooleanAnd,
                        new CodePropertyReferenceExpression(
                            featureContextExpression,
                            nameof(FeatureContext.BeforeFeatureHookFailed))),
                    [new CodeThrowExceptionStatement(
                        new CodeObjectCreateExpression(
                            new CodeTypeReference(typeof(ReqnrollException), CodeTypeReferenceOptions.GlobalReference),
                            new CodePrimitiveExpression("Scenario skipped because of previous before feature hook error")))]);

            // Will generate this for C#:
            // finally {
            //   if (testRunner.FeatureContext?.BeforeFeatureHookFailed)
            //     throw new ReqnrollException("[before feature hook error]");
            //   if (testRunner.FeatureContext == null) { // "Start" the feature if needed
            //     await testRunner.OnFeatureStartAsync(featureInfo);
            //   }
            // }
            CodeStatement onFeatureStartExpression;
            var featureStartMethodInvocation = new CodeMethodInvokeExpression(
                    testRunnerField,
                    nameof(ITestRunner.OnFeatureStartAsync),
                    new CodeVariableReferenceExpression(GeneratorConstants.FEATUREINFO_FIELD));

            if (_codeDomHelper.TargetLanguage != CodeDomProviderLanguage.VB)
            {
                _codeDomHelper.MarkCodeMethodInvokeExpressionAsAwait(featureStartMethodInvocation);
                onFeatureStartExpression = new CodeExpressionStatement(featureStartMethodInvocation);
            }
            else
            // will generate this for VB:
            // Finally
            //   If testRunner.FeatureContext?.BeforeFeatureHookFailed Then
            //     Throw New ReqnrollException("[before feature hook error]")
            //   If testRunner.FeatureContext Is Nothing Then
            //     onFeatureStartTask = testRunner.OnFeatureStartAsync(featureInfo)
            //   EndIf
            // EndTry
            // If onFeatureStartTask IsNot Nothing Then
            //   Await onFeatureStartTask
            // EndIf
            {
                onFeatureStartExpression = new CodeAssignStatement(
                    new CodeVariableReferenceExpression("onFeatureStartTask"),
                    featureStartMethodInvocation);
            }

            var conditionallyExecuteFeatureStartExpressionStatement =
                new CodeConditionStatement(
                    new CodeBinaryOperatorExpression(
                        featureContextExpression,
                        CodeBinaryOperatorType.IdentityEquality,
                        new CodePrimitiveExpression(null)),
                    onFeatureStartExpression);

            testInitializeMethod.Statements.Add(
                new CodeTryCatchFinallyStatement(
                    [conditionallyExecuteOnFeatureEndExpressionStatement],
                    [],
                    [throwErrorOnPreviousFeatureStartError, conditionallyExecuteFeatureStartExpressionStatement]));

            if (_codeDomHelper.TargetLanguage == CodeDomProviderLanguage.VB)
            {
                testInitializeMethod.Statements.Add(
                    new CodeConditionStatement(
                        new CodeBinaryOperatorExpression(
                            new CodeVariableReferenceExpression("onFeatureStartTask"),
                            CodeBinaryOperatorType.IdentityInequality,
                            new CodePrimitiveExpression(null)),
                        new CodeExpressionStatement(new CodeVariableReferenceExpression("await onFeatureStartTask"))));
            }

        }

        private void SetupTestCleanupMethod(TestClassGenerationContext generationContext)
        {
            var testCleanupMethod = generationContext.TestCleanupMethod;

            testCleanupMethod.Attributes = MemberAttributes.Public | MemberAttributes.Final;
            testCleanupMethod.Name = GeneratorConstants.TEST_CLEANUP_NAME;

            _codeDomHelper.MarkCodeMemberMethodAsAsync(testCleanupMethod);

            _testGeneratorProvider.SetTestCleanupMethod(generationContext);

            var testRunnerField = _scenarioPartHelper.GetTestRunnerExpression();

            // Guard when calling AfterScenario when TestRunner is already released (errors or edge cases in a supported test framework)
            // if ((testRunner == null))
            // {
            //     return;
            // }
            var testRunnerNotNullGuard =
                new CodeConditionStatement(new CodeBinaryOperatorExpression(
                    testRunnerField,
                    CodeBinaryOperatorType.IdentityEquality,
                    new CodePrimitiveExpression(null)),
                    new CodeMethodReturnStatement());
            testCleanupMethod.Statements.Add(testRunnerNotNullGuard);

            //await testRunner.OnScenarioEndAsync();
            var onScenarioEndCallExpression = new CodeMethodInvokeExpression(
                testRunnerField,
                nameof(ITestRunner.OnScenarioEndAsync));
            _codeDomHelper.MarkCodeMethodInvokeExpressionAsAwait(onScenarioEndCallExpression);
            var onScenarioEndCallStatement = new CodeExpressionStatement(onScenarioEndCallExpression);

            // "Release" the TestRunner, so that other threads can pick it up
            // global::Reqnroll.TestRunnerManager.ReleaseTestRunner(testRunner);
            var releaseTestRunnerCallStatement = new CodeExpressionStatement(
                new CodeMethodInvokeExpression(
                    new CodeTypeReferenceExpression(new CodeTypeReference(typeof(TestRunnerManager), CodeTypeReferenceOptions.GlobalReference)),
                    nameof(TestRunnerManager.ReleaseTestRunner),
                    testRunnerField));

            // Unassign TestRunner to make sure it won't be reused if the test framework runs AfterScenario multiple times
            // testRunner = null;
            var unasignTestRunnerInstance = new CodeAssignStatement(testRunnerField, new CodePrimitiveExpression(null));

            // add ReleaseTestRunner to the finally block of OnScenarioEndAsync 
            testCleanupMethod.Statements.Add(
                new CodeTryCatchFinallyStatement(
                    [onScenarioEndCallStatement],
                    [],
                    [releaseTestRunnerCallStatement, unasignTestRunnerInstance]
                ));
        }

        private void SetupScenarioInitializeMethod(TestClassGenerationContext generationContext)
        {
            var scenarioInitializeMethod = generationContext.ScenarioInitializeMethod;

            scenarioInitializeMethod.Attributes = MemberAttributes.Public | MemberAttributes.Final;
            scenarioInitializeMethod.Name = GeneratorConstants.SCENARIO_INITIALIZE_NAME;
            scenarioInitializeMethod.Parameters.Add(
                new CodeParameterDeclarationExpression(new CodeTypeReference(typeof(ScenarioInfo), CodeTypeReferenceOptions.GlobalReference), "scenarioInfo"));
            scenarioInitializeMethod.Parameters.Add(
                new CodeParameterDeclarationExpression(new CodeTypeReference(typeof(RuleInfo), CodeTypeReferenceOptions.GlobalReference), "ruleInfo"));

            //testRunner.OnScenarioInitialize(scenarioInfo, ruleInfo);
            var testRunnerField = _scenarioPartHelper.GetTestRunnerExpression();
            scenarioInitializeMethod.Statements.Add(
                new CodeMethodInvokeExpression(
                    testRunnerField,
                    nameof(ITestRunner.OnScenarioInitialize),
                    new CodeVariableReferenceExpression("scenarioInfo"),
                    new CodeVariableReferenceExpression("ruleInfo")));
        }

        private void SetupScenarioStartMethod(TestClassGenerationContext generationContext)
        {
            var scenarioStartMethod = generationContext.ScenarioStartMethod;

            scenarioStartMethod.Attributes = MemberAttributes.Public | MemberAttributes.Final;
            scenarioStartMethod.Name = GeneratorConstants.SCENARIO_START_NAME;

            _codeDomHelper.MarkCodeMemberMethodAsAsync(scenarioStartMethod);

            //await testRunner.OnScenarioStartAsync();
            var testRunnerField = _scenarioPartHelper.GetTestRunnerExpression();
            var expression = new CodeMethodInvokeExpression(
                testRunnerField,
                nameof(ITestRunner.OnScenarioStartAsync));

            _codeDomHelper.MarkCodeMethodInvokeExpressionAsAwait(expression);

            scenarioStartMethod.Statements.Add(expression);
        }
    }
}