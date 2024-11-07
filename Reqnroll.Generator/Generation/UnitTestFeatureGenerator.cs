using System;
using System.CodeDom;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using Reqnroll.Configuration;
using Reqnroll.CucumberMessages.Configuration;
using Reqnroll.CucumberMessages.RuntimeSupport;
using Reqnroll.Generator.CodeDom;
using Reqnroll.Generator.UnitTestConverter;
using Reqnroll.Generator.UnitTestProvider;
using Reqnroll.Parser;
using Reqnroll.Parser.CucmberMessageSupport;
using Reqnroll.Tracing;

namespace Reqnroll.Generator.Generation
{
    [SuppressMessage("ReSharper", "BitwiseOperatorOnEnumWithoutFlags")]
    public class UnitTestFeatureGenerator : IFeatureGenerator
    {
        private readonly CodeDomHelper _codeDomHelper;
        private readonly IDecoratorRegistry _decoratorRegistry;
        private readonly ITraceListener _traceListener;
        private readonly ScenarioPartHelper _scenarioPartHelper;
        private readonly ReqnrollConfiguration _reqnrollConfiguration;
        private readonly IUnitTestGeneratorProvider _testGeneratorProvider;
        private readonly UnitTestMethodGenerator _unitTestMethodGenerator;
        private readonly LinePragmaHandler _linePragmaHandler;
        private readonly ICucumberConfiguration _cucumberConfiguration;

        public UnitTestFeatureGenerator(
            IUnitTestGeneratorProvider testGeneratorProvider,
            CodeDomHelper codeDomHelper,
            ReqnrollConfiguration reqnrollConfiguration,
            IDecoratorRegistry decoratorRegistry,
            ITraceListener traceListener,

            // Adding a dependency on the Cucumber configuration subsystem. Eventually remove this as Cucumber Config is folded into overall Reqnroll Config.
            ICucumberConfiguration cucumberConfiguration)
        {
            _testGeneratorProvider = testGeneratorProvider;
            _codeDomHelper = codeDomHelper;
            _reqnrollConfiguration = reqnrollConfiguration;
            _decoratorRegistry = decoratorRegistry;
            _traceListener = traceListener;
            _linePragmaHandler = new LinePragmaHandler(_reqnrollConfiguration, _codeDomHelper);
            _scenarioPartHelper = new ScenarioPartHelper(_reqnrollConfiguration, _codeDomHelper);
            _unitTestMethodGenerator = new UnitTestMethodGenerator(testGeneratorProvider, decoratorRegistry, _codeDomHelper, _scenarioPartHelper, _reqnrollConfiguration);
            _cucumberConfiguration = cucumberConfiguration;
        }

        public string TestClassNameFormat { get; set; } = "{0}Feature";

        public CodeNamespace GenerateUnitTestFixture(ReqnrollDocument document, string testClassName, string targetNamespace)
        {
            var codeNamespace = CreateNamespace(targetNamespace);
            var feature = document.ReqnrollFeature;

            testClassName = testClassName ?? string.Format(TestClassNameFormat, feature.Name.ToIdentifier());
            var generationContext = CreateTestClassStructure(codeNamespace, testClassName, document);

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
            return codeNamespace;
        }


        private TestClassGenerationContext CreateTestClassStructure(CodeNamespace codeNamespace, string testClassName, ReqnrollDocument document)
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
                _testGeneratorProvider.GetTraits().HasFlag(UnitTestGeneratorTraits.RowTests) && _reqnrollConfiguration.AllowRowTests);
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

            codeNamespace.Imports.Add(new CodeNamespaceImport(GeneratorConstants.REQNROLL_NAMESPACE));
            codeNamespace.Imports.Add(new CodeNamespaceImport("System"));
            codeNamespace.Imports.Add(new CodeNamespaceImport("System.Linq"));
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
            var testRunnerField = new CodeMemberField(_codeDomHelper.GetGlobalizedTypeName(typeof(ITestRunner)), GeneratorConstants.TESTRUNNER_FIELD);
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
                _codeDomHelper.GetGlobalizedTypeName(typeof(FeatureInfo)), GeneratorConstants.FEATUREINFO_FIELD);
            featureInfoField.Attributes |= MemberAttributes.Static;
            featureInfoField.InitExpression = new CodeObjectCreateExpression(_codeDomHelper.GetGlobalizedTypeName(typeof(FeatureInfo)),
                new CodeObjectCreateExpression(typeof(CultureInfo),
                                               new CodePrimitiveExpression(generationContext.Feature.Language)),
                new CodePrimitiveExpression(generationContext.Document.DocumentLocation?.FeatureFolderPath),
                new CodePrimitiveExpression(generationContext.Feature.Name),
                new CodePrimitiveExpression(generationContext.Feature.Description),
                new CodeFieldReferenceExpression(
                    new CodeTypeReferenceExpression(_codeDomHelper.GetGlobalizedTypeName(typeof(ProgrammingLanguage))),
                    _codeDomHelper.TargetLanguage.ToString()),
                new CodeFieldReferenceExpression(null, GeneratorConstants.FEATURE_TAGS_VARIABLE_NAME));

            generationContext.TestClass.Members.Add(featureInfoField);
        }

        private void DeclareFeatureMessagesFactoryMembers(TestClassGenerationContext generationContext)
        {
            // Generation of Cucumber Messages relies on access to the parsed AST. 
            CodeObjectCreateExpression sourceExpression;
            CodeExpression gherkinDocumentExpression;
            CodeExpression picklesExpression;
            CodeDelegateCreateExpression sourceFunc;
            CodeDelegateCreateExpression gherkinDocumentFunc;
            CodeDelegateCreateExpression picklesFunc;

            string sourceFileLocation;

            // Adding three static methods to the class: one each as Factory methods for source, gherkinDocument, and pickles Messages
            // Bodies of these methods are added later inside the try/catch block
                sourceFunc = new CodeDelegateCreateExpression(new CodeTypeReference(typeof(Func<Io.Cucumber.Messages.Types.Source>)), new CodeTypeReferenceExpression(generationContext.TestClass.Name), "SourceFunc");
                var sourceFuncBody = new CodeMemberMethod();
                sourceFuncBody.Attributes = MemberAttributes.Private | MemberAttributes.Static;
                sourceFuncBody.ReturnType = new CodeTypeReference(typeof(Io.Cucumber.Messages.Types.Source));
                sourceFuncBody.Name = sourceFunc.MethodName;
                generationContext.TestClass.Members.Add(sourceFuncBody);

                gherkinDocumentFunc = new CodeDelegateCreateExpression(new CodeTypeReference(typeof(Func<Io.Cucumber.Messages.Types.GherkinDocument>)), new CodeTypeReferenceExpression(generationContext.TestClass.Name), "GherkinDocumentFunc");
                var gherkinDocumentFuncBody = new CodeMemberMethod();
                gherkinDocumentFuncBody.Attributes = MemberAttributes.Private | MemberAttributes.Static;
                gherkinDocumentFuncBody.ReturnType = new CodeTypeReference(typeof(Io.Cucumber.Messages.Types.GherkinDocument));
                gherkinDocumentFuncBody.Name = gherkinDocumentFunc.MethodName;
                generationContext.TestClass.Members.Add(gherkinDocumentFuncBody);

                picklesFunc = new CodeDelegateCreateExpression(new CodeTypeReference(typeof(Func<System.Collections.Generic.IEnumerable<Io.Cucumber.Messages.Types.Pickle>>)), new CodeTypeReferenceExpression(generationContext.TestClass.Name), "PicklesFunc");
                var picklesFuncBody = new CodeMemberMethod();
                picklesFuncBody.Attributes = MemberAttributes.Private | MemberAttributes.Static;
                picklesFuncBody.ReturnType = new CodeTypeReference(typeof(System.Collections.Generic.IEnumerable<Io.Cucumber.Messages.Types.Pickle>));
                picklesFuncBody.Name = picklesFunc.MethodName;
                generationContext.TestClass.Members.Add(picklesFuncBody);
            try
            {
                sourceFileLocation = Path.Combine(generationContext.Document.DocumentLocation.FeatureFolderPath, generationContext.Document.DocumentLocation.SourceFilePath);

                // Cucumber IDs can be UUIDs or stringified integers. This is configurable by the user.
                var IDGenStyle = _cucumberConfiguration.IDGenerationStyle;
                var messageConverter = new CucumberMessagesConverter(IdGeneratorFactory.Create(IDGenStyle));
                var featureSource = Reqnroll.CucumberMessages.PayloadProcessing.Cucumber.CucumberMessageTransformer.ToSource(messageConverter.ConvertToCucumberMessagesSource(generationContext.Document));
                var featureGherkinDocument = messageConverter.ConvertToCucumberMessagesGherkinDocument(generationContext.Document);
                var featurePickles = messageConverter.ConvertToCucumberMessagesPickles(featureGherkinDocument);
                var featureGherkinDocumentMessage = CucumberMessages.PayloadProcessing.Cucumber.CucumberMessageTransformer.ToGherkinDocument(featureGherkinDocument);
                var featurePickleMessages = CucumberMessages.PayloadProcessing.Cucumber.CucumberMessageTransformer.ToPickles(featurePickles);

                // generate a CodeDom expression to create the Source object from the featureSourceMessage
                sourceExpression = new CodeObjectCreateExpression(_codeDomHelper.GetGlobalizedTypeName(typeof(Io.Cucumber.Messages.Types.Source)),
                    new CodePrimitiveExpression(featureSource.Uri),
                    new CodePrimitiveExpression(featureSource.Data),
                    new CodeFieldReferenceExpression(new CodeTypeReferenceExpression(typeof(Io.Cucumber.Messages.Types.SourceMediaType)), featureSource.MediaType.ToString()));

                // generate a CodeDom expression to create the GherkinDocument object from the featureGherkinDocumentMessage
                var gherkinDocumentExpressionGenerator = new CucumberGherkinDocumentExpressionGenerator(_codeDomHelper);
                gherkinDocumentExpression = gherkinDocumentExpressionGenerator.GenerateGherkinDocumentExpression(featureGherkinDocumentMessage);

                // generate a CodeDom expression to create the Pickles object from the featurePickleMessages
                var pickleExpressionGenerator = new CucumberPicklesExpressionGenerator(_codeDomHelper);
                picklesExpression = pickleExpressionGenerator.GeneratePicklesExpression(featurePickleMessages);

                // wrap these expressions in Func<T>

                sourceFuncBody.Statements.Add(new CodeMethodReturnStatement(sourceExpression));

                gherkinDocumentFuncBody.Statements.Add(new CodeMethodReturnStatement(gherkinDocumentExpression));

                picklesFuncBody.Statements.Add(new CodeMethodReturnStatement(picklesExpression));

            }
            catch (Exception e)
            {
                _traceListener.WriteToolOutput($"WARNING: Failed to process Cucumber Pickles. Support for generating Cucumber Messages will be disabled. Exception: {e.Message}");
                // Should any error occur during pickling or serialization of Cucumber Messages, we will abort and not add the Cucumber Messages to the featureInfo.
                // This effectively turns OFF the Cucumber Messages support for this feature.

                // TODO: Add error handling for this case, each factory method should return null;
                return;
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

        // Generation of Cucumber Messages relies on access to the parsed AST. 
        private void PersistStaticCucumberMessagesToFeatureInfo(TestClassGenerationContext generationContext, CodeMemberMethod testClassInitializeMethod)
        {
            CodeObjectCreateExpression sourceExpression;
            CodeExpression gherkinDocumentExpression;
            CodeExpression picklesExpression;
            CodeDelegateCreateExpression sourceFunc;
            CodeDelegateCreateExpression gherkinDocumentFunc;
            CodeDelegateCreateExpression picklesFunc;

            string sourceFileLocation;

            // Create a new method that will be added to the test class. It will be called to initialize the FeatureCucumberMessages property of the FeatureInfo object
            var CucumberMessagesInitializeMethod = new CodeMemberMethod();
            CucumberMessagesInitializeMethod.Attributes = MemberAttributes.Private | MemberAttributes.Static;
            CucumberMessagesInitializeMethod.Name = "InitializeCucumberMessages";
            CucumberMessagesInitializeMethod.Parameters.Add(new CodeParameterDeclarationExpression(_codeDomHelper.GetGlobalizedTypeName(typeof(FeatureInfo)), "featureInfo"));
            generationContext.TestClass.Members.Add(CucumberMessagesInitializeMethod);

            // Create a FeatureLevelCucumberMessages object and add it to featureInfo
            var featureLevelCucumberMessagesExpression = new CodeObjectCreateExpression(_codeDomHelper.GetGlobalizedTypeName(typeof(FeatureLevelCucumberMessages)),
                sourceFunc,
                gherkinDocumentFunc,
                picklesFunc,
                new CodePrimitiveExpression(sourceFileLocation));

            CucumberMessagesInitializeMethod.Statements.Add(
                new CodeAssignStatement(
                    new CodePropertyReferenceExpression(new CodeVariableReferenceExpression("featureInfo"), "FeatureCucumberMessages"),
                    featureLevelCucumberMessagesExpression));

            // Create a CodeMethodInvokeExpression to invoke the CucumberMessagesInitializeMethod
            var invokeCucumberMessagesInitializeMethod = new CodeMethodInvokeExpression(
                null,
                CucumberMessagesInitializeMethod.Name,
                new CodeVariableReferenceExpression("featureInfo"));

            // Add the CodeMethodInvokeExpression to the testClassInitializeMethod statements
            testClassInitializeMethod.Statements.Add(invokeCucumberMessagesInitializeMethod);

        }

        private void SetupTestClassCleanupMethod(TestClassGenerationContext generationContext)
        {
            var testClassCleanupMethod = generationContext.TestClassCleanupMethod;

            testClassCleanupMethod.Attributes = MemberAttributes.Public;
            testClassCleanupMethod.Name = GeneratorConstants.TESTCLASS_CLEANUP_NAME;

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
                new CodeTypeReferenceExpression(_codeDomHelper.GetGlobalizedTypeName(typeof(TestRunnerManager))),
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

            //if (testRunner.FeatureContext != null && !testRunner.FeatureContext.FeatureInfo.Equals(featureInfo))
            //  await testRunner.OnFeatureEndAsync(); // finish if different
            testInitializeMethod.Statements.Add(
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
                        onFeatureEndAsyncExpression)));


            // "Start" the feature if needed

            //if (testRunner.FeatureContext == null) {
            //  await testRunner.OnFeatureStartAsync(featureInfo);
            //}

            var onFeatureStartExpression = new CodeMethodInvokeExpression(
                testRunnerField,
                nameof(ITestRunner.OnFeatureStartAsync),
                new CodeVariableReferenceExpression(GeneratorConstants.FEATUREINFO_FIELD));
            _codeDomHelper.MarkCodeMethodInvokeExpressionAsAwait(onFeatureStartExpression);

            testInitializeMethod.Statements.Add(
                new CodeConditionStatement(
                    new CodeBinaryOperatorExpression(
                        featureContextExpression,
                        CodeBinaryOperatorType.IdentityEquality,
                        new CodePrimitiveExpression(null)),
                    new CodeExpressionStatement(
                        onFeatureStartExpression)));
        }

        private void SetupTestCleanupMethod(TestClassGenerationContext generationContext)
        {
            var testCleanupMethod = generationContext.TestCleanupMethod;

            testCleanupMethod.Attributes = MemberAttributes.Public | MemberAttributes.Final;
            testCleanupMethod.Name = GeneratorConstants.TEST_CLEANUP_NAME;

            _codeDomHelper.MarkCodeMemberMethodAsAsync(testCleanupMethod);

            _testGeneratorProvider.SetTestCleanupMethod(generationContext);

            var testRunnerField = _scenarioPartHelper.GetTestRunnerExpression();

            //await testRunner.OnScenarioEndAsync();
            var expression = new CodeMethodInvokeExpression(
                testRunnerField,
                nameof(ITestRunner.OnScenarioEndAsync));

            _codeDomHelper.MarkCodeMethodInvokeExpressionAsAwait(expression);

            testCleanupMethod.Statements.Add(expression);

            // "Release" the TestRunner, so that other threads can pick it up
            // TestRunnerManager.ReleaseTestRunner(testRunner);
            testCleanupMethod.Statements.Add(
                new CodeMethodInvokeExpression(
                    new CodeTypeReferenceExpression(_codeDomHelper.GetGlobalizedTypeName(typeof(TestRunnerManager))),
                    nameof(TestRunnerManager.ReleaseTestRunner),
                    testRunnerField));
        }

        private void SetupScenarioInitializeMethod(TestClassGenerationContext generationContext)
        {
            var scenarioInitializeMethod = generationContext.ScenarioInitializeMethod;

            scenarioInitializeMethod.Attributes = MemberAttributes.Public | MemberAttributes.Final;
            scenarioInitializeMethod.Name = GeneratorConstants.SCENARIO_INITIALIZE_NAME;
            scenarioInitializeMethod.Parameters.Add(
                new CodeParameterDeclarationExpression(_codeDomHelper.GetGlobalizedTypeName(typeof(ScenarioInfo)), "scenarioInfo"));

            //testRunner.OnScenarioInitialize(scenarioInfo);
            var testRunnerField = _scenarioPartHelper.GetTestRunnerExpression();
            scenarioInitializeMethod.Statements.Add(
                new CodeMethodInvokeExpression(
                    testRunnerField,
                    nameof(ITestRunner.OnScenarioInitialize),
                    new CodeVariableReferenceExpression("scenarioInfo")));
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