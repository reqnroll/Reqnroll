using Io.Cucumber.Messages.Types;
using Reqnroll.Configuration;
using Reqnroll.Formatters.PayloadProcessing;
using Reqnroll.Formatters.RuntimeSupport;
using Reqnroll.Generator.CodeDom;
using Reqnroll.Generator.UnitTestConverter;
using Reqnroll.Generator.UnitTestProvider;
using Reqnroll.Parser;
using Reqnroll.Parser.CucumberMessages;
using Reqnroll.Tracing;
using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace Reqnroll.Generator.Generation;

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

    public UnitTestFeatureGenerationResult GenerateUnitTestFixture(ReqnrollDocument document, string testClassName, string targetNamespace)
    {
        var codeNamespace = CreateNamespace(targetNamespace);
        var feature = document.ReqnrollFeature;

        testClassName ??= string.Format(TestClassNameFormat, feature.Name.ToIdentifier());
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

        return new UnitTestFeatureGenerationResult(codeNamespace, generationContext.FeatureMessages, generationContext.FeatureMessagesResourceName, generationContext.GenerationWarnings);
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
            _codeDomHelper.CreateMethod(testClass),
            _testGeneratorProvider.GetTraits().HasFlag(UnitTestGeneratorTraits.RowTests) && _reqnrollConfiguration.AllowRowTests,
            _reqnrollConfiguration.DisableFriendlyTestNames);
    }

    private CodeNamespace CreateNamespace(string targetNamespace)
    {
        targetNamespace ??= GeneratorConstants.DEFAULT_NAMESPACE;

        if (!targetNamespace.StartsWith("global", StringComparison.CurrentCultureIgnoreCase))
        {
            switch (_codeDomHelper.TargetLanguage)
            {
                case CodeDomProviderLanguage.VB:
                    targetNamespace = $"GlobalVBNetNamespace.{targetNamespace}";
                    break;
            }
        }

        return new CodeNamespace(targetNamespace);
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
                                                                         new CodeMethodInvokeExpression(null, generationContext.CucumberMessagesInitializationMethod.Name));

        generationContext.TestClass.Members.Add(featureInfoField);
    }

    private void DeclareFeatureMessagesFactoryMembers(TestClassGenerationContext generationContext)
    {
        string GetFeatureMessagesResourceName()
        {
            try
            {
                var fileName = Path.GetFileName(generationContext.Document.SourceFilePath);
                var folderPath = generationContext.Document.DocumentLocation.FeatureFolderPath;
                var featureFilePathWithSlash = string.IsNullOrEmpty(folderPath) || folderPath == "." ? fileName : $"{folderPath}/{fileName}";
                return featureFilePathWithSlash + ".ndjson";
            }
            catch (System.Exception)
            {
                return null;
            }
        }

        // Generation of Cucumber Messages relies on access to the parsed AST from 'generationContext.Document'

        // This method will be called to provide the FeatureCucumberMessages property value of the FeatureInfo object when that object is constructed
        var cucumberMessagesInitializationMethod = generationContext.CucumberMessagesInitializationMethod;
        cucumberMessagesInitializationMethod.Attributes = MemberAttributes.Private | MemberAttributes.Static;
        cucumberMessagesInitializationMethod.Name = GeneratorConstants.FEATURE_MESSAGES_INITIALIZATION_NAME;
        cucumberMessagesInitializationMethod.ReturnType = new CodeTypeReference(typeof(FeatureLevelCucumberMessages), CodeTypeReferenceOptions.GlobalReference);

        int envelopeCount;
        try
        {
            var featureSource = CucumberMessagesConverter.ConvertToCucumberMessagesSource(generationContext.Document);
            generationContext.FeatureMessagesResourceName = GetFeatureMessagesResourceName();
            var idGeneratorSeed = featureSource.Uri + featureSource.Data;
            var messageConverter = new CucumberMessagesConverter(new DeterministicIdGenerator(idGeneratorSeed));
            var featureGherkinDocumentMessage = messageConverter.ConvertToCucumberMessagesGherkinDocument(generationContext.Document);
            var featurePickleMessages = messageConverter.ConvertToCucumberMessagesPickles(featureGherkinDocumentMessage);
            // Collect all envelopes
            var envelopes = new List<Envelope>
            {
                Envelope.Create(featureSource),
                Envelope.Create(featureGherkinDocumentMessage)
            };
            envelopes.AddRange(featurePickleMessages.Select(Envelope.Create));
            envelopeCount = envelopes.Count;

            // Serialize each envelope and append into a ndjson format
            generationContext.FeatureMessages = string.Join(Environment.NewLine, envelopes.Select(NdjsonSerializer.Serialize));

        }
        catch (System.Exception e)
        {
            generationContext.GenerationWarnings.Add($"WARNING: Failed to process Cucumber Pickles. Support for generating Cucumber Messages will be disabled. Exception: {e.Message}");
            // Should any error occur during pickling or serialization of Cucumber Messages, we will abort and not add the Cucumber Messages to the featureInfo.
            // This effectively turns OFF the Cucumber Messages support for this feature.
            generationContext.FeatureMessages = null;
            generationContext.FeatureMessagesResourceName = null;
            envelopeCount = 0;
        }

        // Create a FeatureLevelCucumberMessages object and add it to featureInfo
        var featureLevelCucumberMessagesExpression = new CodeObjectCreateExpression(
            new CodeTypeReference(typeof(FeatureLevelCucumberMessages), CodeTypeReferenceOptions.GlobalReference),
            new CodePrimitiveExpression(generationContext.FeatureMessagesResourceName),
            new CodePrimitiveExpression(envelopeCount));

        cucumberMessagesInitializationMethod.Statements.Add(
            new CodeMethodReturnStatement(
                featureLevelCucumberMessagesExpression));
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
        var unassignTestRunnerInstance = new CodeAssignStatement(testRunnerField, new CodePrimitiveExpression(null));

        // add ReleaseTestRunner to the finally block of OnScenarioEndAsync 
        testCleanupMethod.Statements.Add(
            new CodeTryCatchFinallyStatement(
                [onScenarioEndCallStatement],
                [],
                [releaseTestRunnerCallStatement, unassignTestRunnerInstance]
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