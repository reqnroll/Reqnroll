using System;
using System.CodeDom;
using System.Globalization;
using System.Linq;
using System.Reflection;
using Gherkin.CucumberMessages;
using Reqnroll.Configuration;
using Reqnroll.CucumberMessages;
using Reqnroll.Generator.CodeDom;
using Reqnroll.Generator.UnitTestConverter;
using Reqnroll.Generator.UnitTestProvider;
using Reqnroll.Parser;
using Reqnroll.Tracing;

namespace Reqnroll.Generator.Generation
{
    public class UnitTestFeatureGenerator : IFeatureGenerator
    {
        private readonly CodeDomHelper _codeDomHelper;
        private readonly IDecoratorRegistry _decoratorRegistry;
        private readonly ScenarioPartHelper _scenarioPartHelper;
        private readonly ReqnrollConfiguration _reqnrollConfiguration;
        private readonly IUnitTestGeneratorProvider _testGeneratorProvider;
        private readonly UnitTestMethodGenerator _unitTestMethodGenerator;
        private readonly LinePragmaHandler _linePragmaHandler;

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

            var featureTagsField = new CodeMemberField(typeof(string[]), GeneratorConstants.FEATURE_TAGS_VARIABLE_NAME);
            featureTagsField.Attributes |= MemberAttributes.Static;
            featureTagsField.InitExpression = _scenarioPartHelper.GetStringArrayExpression(generationContext.Feature.Tags);

            generationContext.TestClass.Members.Add(featureTagsField);
        }

        private CodeMemberField DeclareTestRunnerMember(CodeTypeDeclaration type)
        {
            var testRunnerField = new CodeMemberField(_codeDomHelper.GetGlobalizedTypeName(typeof(ITestRunner)), GeneratorConstants.TESTRUNNER_FIELD);
            type.Members.Add(testRunnerField);
            return testRunnerField;
        }

        private void SetupTestClassInitializeMethod(TestClassGenerationContext generationContext)
        {
            var testClassInitializeMethod = generationContext.TestClassInitializeMethod;

            testClassInitializeMethod.Attributes = MemberAttributes.Public;
            testClassInitializeMethod.Name = GeneratorConstants.TESTCLASS_INITIALIZE_NAME;

            _codeDomHelper.MarkCodeMemberMethodAsAsync(testClassInitializeMethod);

            _testGeneratorProvider.SetTestClassInitializeMethod(generationContext);

            //testRunner = TestRunnerManager.GetTestRunnerForAssembly(null, [test_worker_id]);
            var testRunnerField = _scenarioPartHelper.GetTestRunnerExpression();

            var getTestRunnerExpression = new CodeMethodInvokeExpression(
                new CodeTypeReferenceExpression(_codeDomHelper.GetGlobalizedTypeName(typeof(TestRunnerManager))),
                nameof(TestRunnerManager.GetTestRunnerForAssembly));

            testClassInitializeMethod.Statements.Add(
                new CodeAssignStatement(
                    testRunnerField,
                    getTestRunnerExpression));

            //FeatureInfo featureInfo = new FeatureInfo("xxxx");
            testClassInitializeMethod.Statements.Add(
                new CodeVariableDeclarationStatement(_codeDomHelper.GetGlobalizedTypeName(typeof(FeatureInfo)), "featureInfo",
                    new CodeObjectCreateExpression(_codeDomHelper.GetGlobalizedTypeName(typeof(FeatureInfo)),
                        new CodeObjectCreateExpression(typeof(CultureInfo),
                            new CodePrimitiveExpression(generationContext.Feature.Language)),
                        new CodePrimitiveExpression(generationContext.Document.DocumentLocation?.FeatureFolderPath),
                        new CodePrimitiveExpression(generationContext.Feature.Name),
                        new CodePrimitiveExpression(generationContext.Feature.Description),
                        new CodeFieldReferenceExpression(
                            new CodeTypeReferenceExpression(_codeDomHelper.GetGlobalizedTypeName(typeof(Reqnroll.ProgrammingLanguage))),
                            _codeDomHelper.TargetLanguage.ToString()),
                        new CodeFieldReferenceExpression(null, GeneratorConstants.FEATURE_TAGS_VARIABLE_NAME))));

            PersistStaticCucumberMessagesToFeatureInfo(generationContext, testClassInitializeMethod);

            //await testRunner.OnFeatureStartAsync(featureInfo);
            var onFeatureStartExpression = new CodeMethodInvokeExpression(
                testRunnerField,
                nameof(ITestRunner.OnFeatureStartAsync),
                new CodeVariableReferenceExpression("featureInfo"));

            _codeDomHelper.MarkCodeMethodInvokeExpressionAsAwait(onFeatureStartExpression);

            testClassInitializeMethod.Statements.Add(onFeatureStartExpression);
        }

        private void PersistStaticCucumberMessagesToFeatureInfo(TestClassGenerationContext generationContext, CodeMemberMethod testClassInitializeMethod)
        {
            string featureSourceMessageString = null; 
            string featureGherkinDocumentMessageString = null;
            string featurePickleMessagesString = null;
            try 
            {
                //Generate Feature level Cucumber Messages, serialize them to strings, create a FeatureLevelCucumberMessages object and add it to featureInfo
                var messageConverter = new CucumberMessagesConverter(new IncrementingIdGenerator());
                var featureSourceMessage = messageConverter.ConvertToCucumberMessagesSource(generationContext.Document);
                var featureGherkinDocumentMessage = messageConverter.ConvertToCucumberMessagesGherkinDocument(generationContext.Document);
                var featurePickleMessages = messageConverter.ConvertToCucumberMessagesPickles(featureGherkinDocumentMessage);

                // Serialize the Cucumber Messages to strings
                featureSourceMessageString = System.Text.Json.JsonSerializer.Serialize(featureSourceMessage);
                featureGherkinDocumentMessageString = System.Text.Json.JsonSerializer.Serialize(featureGherkinDocumentMessage);
                featurePickleMessagesString = System.Text.Json.JsonSerializer.Serialize(featurePickleMessages);
            }
            catch
            {
                // Should any error occur during pickling or serialization of Cucumber Messages, we will abort and not add the Cucumber Messages to the featureInfo.
                return;            
            }
            // Create a new method that will be added to the test class. It will be called to initialize the FeatureCucumberMessages property of the FeatureInfo object
            var CucumberMessagesInitializeMethod = new CodeMemberMethod();
            CucumberMessagesInitializeMethod.Attributes = MemberAttributes.Private | MemberAttributes.Static;
            CucumberMessagesInitializeMethod.Name = "InitializeCucumberMessages";
            CucumberMessagesInitializeMethod.Parameters.Add(new CodeParameterDeclarationExpression(_codeDomHelper.GetGlobalizedTypeName(typeof(FeatureInfo)), "featureInfo"));
            generationContext.TestClass.Members.Add(CucumberMessagesInitializeMethod);


            // Create a FeatureLevelCucumberMessages object and add it to featureInfo
            var featureLevelCucumberMessagesExpression = new CodeObjectCreateExpression(_codeDomHelper.GetGlobalizedTypeName(typeof(FeatureLevelCucumberMessages)),
                new CodePrimitiveExpression(featureSourceMessageString),
                new CodePrimitiveExpression(featureGherkinDocumentMessageString),
                new CodePrimitiveExpression(featurePickleMessagesString));
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

            var testRunnerField = _scenarioPartHelper.GetTestRunnerExpression();

            // await testRunner.OnFeatureEndAsync();
            var expression = new CodeMethodInvokeExpression(
                testRunnerField,
                nameof(ITestRunner.OnFeatureEndAsync));

            _codeDomHelper.MarkCodeMethodInvokeExpressionAsAwait(expression);

            testClassCleanupMethod.Statements.Add(expression);

            // 
            testClassCleanupMethod.Statements.Add(
                new CodeMethodInvokeExpression(
                    new CodeTypeReferenceExpression(_codeDomHelper.GetGlobalizedTypeName(typeof(TestRunnerManager))),
                    nameof(TestRunnerManager.ReleaseTestRunner),
                    testRunnerField));

            // testRunner = null;
            testClassCleanupMethod.Statements.Add(
                new CodeAssignStatement(
                    testRunnerField,
                    new CodePrimitiveExpression(null)));
        }

        private void SetupTestInitializeMethod(TestClassGenerationContext generationContext)
        {
            var testInitializeMethod = generationContext.TestInitializeMethod;

            testInitializeMethod.Attributes = MemberAttributes.Public | MemberAttributes.Final;
            testInitializeMethod.Name = GeneratorConstants.TEST_INITIALIZE_NAME;

            _codeDomHelper.MarkCodeMemberMethodAsAsync(testInitializeMethod);
            
            _testGeneratorProvider.SetTestInitializeMethod(generationContext);
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