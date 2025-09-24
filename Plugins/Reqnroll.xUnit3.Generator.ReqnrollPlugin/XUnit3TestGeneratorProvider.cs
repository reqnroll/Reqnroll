using Reqnroll.BoDi;
using Reqnroll.Generator;
using Reqnroll.Generator.CodeDom;
using Reqnroll.Generator.UnitTestProvider;
using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Linq;

namespace Reqnroll.xUnit3.Generator.ReqnrollPlugin;

public sealed class XUnit3TestGeneratorProvider(CodeDomHelper codeDomHelper)
    : IUnitTestGeneratorProvider
{
    private CodeTypeDeclaration _currentFixtureDataTypeDeclaration;
    private readonly CodeTypeReference _objectCodeTypeReference = new(typeof(object));
    private readonly CodeDomHelper _codeDomHelper = codeDomHelper;

    private const string IASYNCLIFETIME_INTERFACE = "Xunit.IAsyncLifetime";
    private const string IASYNC_DISPOSABLE_INTERFACE = "System.IAsyncDisposable";
    private const string OUTPUT_INTERFACE = "Xunit.ITestOutputHelper";
    private const string FACT_ATTRIBUTE = "Xunit.FactAttribute";
    private const string THEORY_ATTRIBUTE = "Xunit.TheoryAttribute";
    private const string INLINEDATA_ATTRIBUTE = "Xunit.InlineDataAttribute";
    private const string ICLASSFIXTURE_INTERFACE = "Xunit.IClassFixture";
    private const string TRAIT_ATTRIBUTE = "Xunit.TraitAttribute";
    private const string FEATURE_TITLE = "FeatureTitle";
    private const string SKIP_PROPERTY_NAME = "Skip";
    private const string IGNORED_REASON = "Ignored";
    private const string DISPOSE_ASYNC_METHOD = "DisposeAsync";
    private const string INITIALIZE_ASYNC_METHOD = "InitializeAsync";
    private const string CATEGORY_PROPERTY_NAME = "Category";
    private const string TRAIT_DESCRIPTION_ATTRIBUTE = "Description";
    private const string DISPLAY_NAME_PROPERTY = "DisplayName";
    private const string IGNORE_TEST_CLASS = "IgnoreTestClass";
    private const string NONPARALLELIZABLE_COLLECTION_NAME = "ReqnrollNonParallelizableFeatures";
    private const string COLLECTION_ATTRIBUTE = "Xunit.CollectionAttribute";

    public UnitTestGeneratorTraits GetTraits() => UnitTestGeneratorTraits.RowTests;

    public void SetTestClass(TestClassGenerationContext generationContext, string featureTitle, string featureDescription)
    {
        _currentFixtureDataTypeDeclaration = _codeDomHelper.CreateGeneratedTypeDeclaration("FixtureData");

        // have to add the explicit object base class because of VB.NET
        _currentFixtureDataTypeDeclaration.BaseTypes.Add(typeof(object));
        var asyncLifetimeInterface = new CodeTypeReference(IASYNCLIFETIME_INTERFACE);
        _currentFixtureDataTypeDeclaration.BaseTypes.Add(asyncLifetimeInterface);

        generationContext.TestClass.Members.Add(_currentFixtureDataTypeDeclaration);

        var fixtureDataType =
            _codeDomHelper.CreateNestedTypeReference(generationContext.TestClass, _currentFixtureDataTypeDeclaration.Name);

        var useFixtureType = CreateFixtureInterface(generationContext, fixtureDataType);

        generationContext.TestClass.BaseTypes.Add(_objectCodeTypeReference);
        generationContext.TestClass.BaseTypes.Add(useFixtureType);
    }

    public void SetTestClassCategories(TestClassGenerationContext generationContext, IEnumerable<string> featureCategories)
    {
        foreach (string category in featureCategories)
        {
            SetProperty(generationContext.TestClass, CATEGORY_PROPERTY_NAME, category);
        }
    }

    public void SetTestClassIgnore(TestClassGenerationContext generationContext)
    {
        // The individual tests have to get Skip argument in their attributes
        // xUnit does not provide a way to Skip an entire test class
        // This is handled in FinalizeTestClass
            
        // Store in custom data that the test class should be ignored
        generationContext.CustomData.Add(IGNORE_TEST_CLASS, true);
    }

    public void FinalizeTestClass(TestClassGenerationContext generationContext)
    {
        IgnoreFeature(generationContext);
        
        // testRunner.ScenarioContext.ScenarioContainer.RegisterInstanceAs<ITestOutputHelper>(_testOutputHelper);
        generationContext.ScenarioInitializeMethod.Statements.Add(
            new CodeMethodInvokeExpression(
                new CodeMethodReferenceExpression(
                    new CodePropertyReferenceExpression(
                        new CodePropertyReferenceExpression(
                            new CodeFieldReferenceExpression(null, generationContext.TestRunnerField.Name),
                            nameof(ScenarioContext)),
                        nameof(ScenarioContext.ScenarioContainer)),
                    nameof(IObjectContainer.RegisterInstanceAs),
                    new CodeTypeReference(OUTPUT_INTERFACE)),
                new CodeVariableReferenceExpression("_testOutputHelper")));
    }

    public void SetTestClassNonParallelizable(TestClassGenerationContext generationContext)
    {
        _codeDomHelper.AddAttribute(
            generationContext.TestClass,
            COLLECTION_ATTRIBUTE,
            new CodeAttributeArgument(new CodePrimitiveExpression(NONPARALLELIZABLE_COLLECTION_NAME)));
    }

    public void SetTestMethodNonParallelizable(TestClassGenerationContext generationContext, CodeMemberMethod testMethod)
    {
        // xUnit does not support method-level parallelization
    }

    public void SetTestClassInitializeMethod(TestClassGenerationContext generationContext)
    {
        // ReSharper disable once BitwiseOperatorOnEnumWithoutFlags
        generationContext.TestClassInitializeMethod.Attributes |= MemberAttributes.Static;

        // ValueTask IAsyncLifetime.InitializeAsync() { <fixtureSetupMethod>(); }
        var initializeMethod = new CodeMemberMethod
        {
            PrivateImplementationType = new CodeTypeReference(IASYNCLIFETIME_INTERFACE),
            Name = INITIALIZE_ASYNC_METHOD,
            ReturnType = new CodeTypeReference(typeof(System.Threading.Tasks.ValueTask))
        };

        _codeDomHelper.MarkCodeMemberMethodAsAsync(initializeMethod);

        _currentFixtureDataTypeDeclaration.Members.Add(initializeMethod);

        var expression = new CodeMethodInvokeExpression(
            new CodeTypeReferenceExpression(new CodeTypeReference(generationContext.TestClass.Name)),
            generationContext.TestClassInitializeMethod.Name);

        // Skip awaiting for VB when the method returns ValueTask
        if (!(_codeDomHelper.TargetLanguage == CodeDomProviderLanguage.VB &&
              initializeMethod.ReturnType.BaseType.Contains("ValueTask")))
        {
            _codeDomHelper.MarkCodeMethodInvokeExpressionAsAwait(expression);
        }

        initializeMethod.Statements.Add(expression);
    }

    public void SetTestClassCleanupMethod(TestClassGenerationContext generationContext)
    {
        // ReSharper disable once BitwiseOperatorOnEnumWithoutFlags
        generationContext.TestClassCleanupMethod.Attributes |= MemberAttributes.Static;

        // ValueTask IAsyncDisposable.DisposeAsync() { <fixtureTearDownMethod>(); }
        var disposeMethod = new CodeMemberMethod
        {
            PrivateImplementationType = new CodeTypeReference(IASYNC_DISPOSABLE_INTERFACE),
            Name = DISPOSE_ASYNC_METHOD,
            ReturnType = new CodeTypeReference(typeof(System.Threading.Tasks.ValueTask))
        };

        _codeDomHelper.MarkCodeMemberMethodAsAsync(disposeMethod);

        _currentFixtureDataTypeDeclaration.Members.Add(disposeMethod);

        var expression = new CodeMethodInvokeExpression(
            new CodeTypeReferenceExpression(new CodeTypeReference(generationContext.TestClass.Name)),
            generationContext.TestClassCleanupMethod.Name);

        // Skip awaiting for VB when the method returns ValueTask
        if (!(_codeDomHelper.TargetLanguage == CodeDomProviderLanguage.VB &&
              disposeMethod.ReturnType.BaseType.Contains("ValueTask")))
        {
            _codeDomHelper.MarkCodeMethodInvokeExpressionAsAwait(expression);
        }

        disposeMethod.Statements.Add(expression);
    }

    public void SetTestInitializeMethod(TestClassGenerationContext generationContext)
    {
        // xUnit uses a parameterless constructor & IAsyncLifetime.InitializeAsync
        // public <_currentTestTypeDeclaration>() { <memberMethod>(); }
        var ctorMethod = new CodeConstructor
        {
            Attributes = MemberAttributes.Public
        };
        generationContext.TestClass.Members.Add(ctorMethod);

        SetTestConstructor(generationContext, ctorMethod);

        var asyncLifetimeInterface = new CodeTypeReference(IASYNCLIFETIME_INTERFACE);
        generationContext.TestClass.BaseTypes.Add(asyncLifetimeInterface);

        // ValueTask IAsyncLifetime.InitializeAsync() { <memberMethod>(); }
        var initializeMethod = new CodeMemberMethod
        {
            Attributes = MemberAttributes.Public,
            PrivateImplementationType = new CodeTypeReference(IASYNCLIFETIME_INTERFACE),
            Name = INITIALIZE_ASYNC_METHOD,
            ReturnType = new CodeTypeReference(typeof(System.Threading.Tasks.ValueTask))
        };

        _codeDomHelper.MarkCodeMemberMethodAsAsync(initializeMethod);

        generationContext.TestClass.Members.Add(initializeMethod);

        SetTestInitializeMethod(generationContext, initializeMethod);
    }

    public void SetTestCleanupMethod(TestClassGenerationContext generationContext)
    {
        // ValueTask IAsyncLifetime.DisposeAsync() { <memberMethod>(); }
        var disposeMethod = new CodeMemberMethod
        {
            PrivateImplementationType = new CodeTypeReference(IASYNC_DISPOSABLE_INTERFACE),
            Name = DISPOSE_ASYNC_METHOD,
            ReturnType = new CodeTypeReference(typeof(System.Threading.Tasks.ValueTask))
        };

        _codeDomHelper.MarkCodeMemberMethodAsAsync(disposeMethod);

        generationContext.TestClass.Members.Add(disposeMethod);

        var expression = new CodeMethodInvokeExpression(
            new CodeThisReferenceExpression(),
            generationContext.TestCleanupMethod.Name);

        // Skip awaiting for VB when the method returns ValueTask
        if (!(_codeDomHelper.TargetLanguage == CodeDomProviderLanguage.VB &&
              disposeMethod.ReturnType.BaseType.Contains("ValueTask")))
        {
            _codeDomHelper.MarkCodeMethodInvokeExpressionAsAwait(expression);
        }

        disposeMethod.Statements.Add(expression);
    }

    public void SetTestMethod(TestClassGenerationContext generationContext, CodeMemberMethod testMethod, string friendlyTestName)
    {
        _codeDomHelper.AddAttribute(testMethod, FACT_ATTRIBUTE, new CodeAttributeArgument(DISPLAY_NAME_PROPERTY, new CodePrimitiveExpression(friendlyTestName)));
        SetProperty(testMethod, FEATURE_TITLE, generationContext.Feature.Name);
        SetDescription(testMethod, friendlyTestName);
    }

    public void SetTestMethodCategories(TestClassGenerationContext generationContext, CodeMemberMethod testMethod, IEnumerable<string> scenarioCategories)
    {
        foreach (string str in scenarioCategories)
        {
            SetProperty(testMethod, CATEGORY_PROPERTY_NAME, str);
        }
    }

    public void SetTestMethodIgnore(TestClassGenerationContext generationContext, CodeMemberMethod testMethod)
    {
        var factAttr = testMethod.CustomAttributes
                                 .OfType<CodeAttributeDeclaration>()
                                 .FirstOrDefault(codeAttributeDeclaration => codeAttributeDeclaration.Name == FACT_ATTRIBUTE);

        // set [FactAttribute(Skip="reason")]
        factAttr?.Arguments.Add(new CodeAttributeArgument(SKIP_PROPERTY_NAME, new CodePrimitiveExpression(IGNORED_REASON)));

        var theoryAttr = testMethod.CustomAttributes
                                   .OfType<CodeAttributeDeclaration>()
                                   .FirstOrDefault(codeAttributeDeclaration => codeAttributeDeclaration.Name == THEORY_ATTRIBUTE);

        // set [TheoryAttribute(Skip="reason")]
        theoryAttr?.Arguments.Add(new CodeAttributeArgument(SKIP_PROPERTY_NAME, new CodePrimitiveExpression(IGNORED_REASON)));
    }

    public void SetRowTest(TestClassGenerationContext generationContext, CodeMemberMethod testMethod, string scenarioTitle)
    {
        _codeDomHelper.AddAttribute(testMethod, THEORY_ATTRIBUTE, new CodeAttributeArgument(DISPLAY_NAME_PROPERTY, new CodePrimitiveExpression(scenarioTitle)));
        SetProperty(testMethod, FEATURE_TITLE, generationContext.Feature.Name);
        SetDescription(testMethod, scenarioTitle);
    }

    // Set InlineDataAttribute for row test
    public void SetRow(TestClassGenerationContext generationContext, CodeMemberMethod testMethod, IEnumerable<string> arguments, IEnumerable<string> tags, bool isIgnored)
    {
        if (isIgnored)
        {
            return;
        }

        var args = arguments.Select(arg => new CodeAttributeArgument(new CodePrimitiveExpression(arg))).ToList();

        args.Add(
            new CodeAttributeArgument(
                new CodeArrayCreateExpression(typeof(string[]), tags.Select(CodeExpression (t) => new CodePrimitiveExpression(t)).ToArray())));

        _codeDomHelper.AddAttribute(testMethod, INLINEDATA_ATTRIBUTE, args.ToArray());
    }

    public void SetTestMethodAsRow(TestClassGenerationContext generationContext, CodeMemberMethod testMethod, string scenarioTitle, string exampleSetName, string variantName, IEnumerable<KeyValuePair<string, string>> arguments)
    {
        // doing nothing since we support RowTest
    }

    public void MarkCodeMethodInvokeExpressionAsAwait(CodeMethodInvokeExpression expression)
    {
        throw new NotImplementedException();
    }

    private CodeTypeReference CreateFixtureInterface(TestClassGenerationContext generationContext, CodeTypeReference fixtureDataType)
    {
        // Add a field for the ITestOutputHelper
        generationContext.TestClass.Members.Add(new CodeMemberField(OUTPUT_INTERFACE, "_testOutputHelper"));

        // Store the fixture data type for later use in constructor
        generationContext.CustomData.Add("fixtureData", fixtureDataType);

        return new CodeTypeReference(ICLASSFIXTURE_INTERFACE, fixtureDataType);
    }

    private void SetTestConstructor(TestClassGenerationContext generationContext, CodeConstructor constructor)
    {
        constructor.Parameters.Add(
            new CodeParameterDeclarationExpression((CodeTypeReference)generationContext.CustomData["fixtureData"], "fixtureData"));
        constructor.Parameters.Add(
            new CodeParameterDeclarationExpression(OUTPUT_INTERFACE, "testOutputHelper"));

        constructor.Statements.Add(
            new CodeAssignStatement(
                new CodeFieldReferenceExpression(new CodeThisReferenceExpression(), "_testOutputHelper"),
                new CodeVariableReferenceExpression("testOutputHelper")));
    }

    private CodeStatement GetTryCatchStatementWithCombinedErrors(CodeExpression tryExpression, CodeExpression catchExpression)
    {
        var tryStatement = new CodeExpressionStatement(tryExpression);
        var catchStatement = new CodeExpressionStatement(catchExpression);

        // try { <tryExpression>; }
        // catch (Exception e1) {
        //   try { <catchExpression>; }
        //   catch (Exception e2) {
        //     throw new AggregateException(e1.Message, e1, e2);
        //   }
        //   throw;
        // }

        return new CodeTryCatchFinallyStatement(
            [tryStatement],
            [
                new CodeCatchClause(
                        "e1",
                        new CodeTypeReference(typeof(Exception)),
                        new CodeTryCatchFinallyStatement(
                            [catchStatement],
                            [
                                new CodeCatchClause(
                                    "e2",
                                    new CodeTypeReference(typeof(Exception)),
                                    new CodeThrowExceptionStatement(new CodeObjectCreateExpression(typeof(AggregateException), new CodePrimitiveExpression("Test initialization failed"), new CodeVariableReferenceExpression("e1"), new CodeVariableReferenceExpression("e2"))))
                                ]),
                        new CodeThrowExceptionStatement())
            ]);
    }

    private void SetTestInitializeMethod(TestClassGenerationContext generationContext, CodeMemberMethod method)
    {
        var callTestInitializeMethodExpression = new CodeMethodInvokeExpression(
            new CodeThisReferenceExpression(),
            generationContext.TestInitializeMethod.Name);

        // Skip awaiting for VB when the method returns ValueTask
        if (!(_codeDomHelper.TargetLanguage == CodeDomProviderLanguage.VB &&
             method.ReturnType.BaseType.Contains("ValueTask")))
        {
            _codeDomHelper.MarkCodeMethodInvokeExpressionAsAwait(callTestInitializeMethodExpression);
        }

        // xUnit (v3) does not invoke the test level dispose through the IAsyncLifetime if
        // there is an error during the test initialization. So we need to call it manually, 
        // otherwise the test runner will not be released.
        var callDisposeException = new CodeMethodInvokeExpression(
            new CodeCastExpression(new CodeTypeReference(IASYNCLIFETIME_INTERFACE), new CodeThisReferenceExpression()),
            DISPOSE_ASYNC_METHOD);
        _codeDomHelper.MarkCodeMethodInvokeExpressionAsAwait(callDisposeException);

        method.Statements.Add(
            GetTryCatchStatementWithCombinedErrors(callTestInitializeMethodExpression, callDisposeException));
    }
    
    private void IgnoreFeature(TestClassGenerationContext generationContext)
    {
        bool featureHasIgnoreTag = generationContext.CustomData.TryGetValue(IGNORE_TEST_CLASS, out object featureHasIgnoreTagValue) && (bool)featureHasIgnoreTagValue;

        if (!featureHasIgnoreTag)
        {
            return;
        }

        //Ignore all test methods
        foreach (CodeTypeMember member in generationContext.TestClass.Members)
        {
            if (member is CodeMemberMethod method && !IsTestMethodAlreadyIgnored(method))
            {
                SetTestMethodIgnore(generationContext, method);
            }
        }

        //Clear FixtureData method statements to skip the Before/AfterFeature hooks
        foreach (CodeTypeMember member in _currentFixtureDataTypeDeclaration.Members)
        {
            if (member is CodeMemberMethod method)
            {
                method.Statements.Clear();
            }
        }
    }

    private bool IsTestMethodAlreadyIgnored(CodeMemberMethod testMethod)
    {
        var factAttr = testMethod.CustomAttributes.OfType<CodeAttributeDeclaration>()
                                 .FirstOrDefault(codeAttributeDeclaration => codeAttributeDeclaration.Name == FACT_ATTRIBUTE);

        bool? hasIgnoredFact = factAttr?.Arguments
                                       .OfType<CodeAttributeArgument>()
                                       .Any(x => string.Equals(x.Name, SKIP_PROPERTY_NAME, StringComparison.InvariantCultureIgnoreCase));

        var theoryAttr = testMethod.CustomAttributes
                                   .OfType<CodeAttributeDeclaration>()
                                   .FirstOrDefault(codeAttributeDeclaration => codeAttributeDeclaration.Name == THEORY_ATTRIBUTE);

        bool? hasIgnoredTheory = theoryAttr?.Arguments
                                           .OfType<CodeAttributeArgument>()
                                           .Any(x => string.Equals(x.Name, SKIP_PROPERTY_NAME, StringComparison.InvariantCultureIgnoreCase));

        return hasIgnoredFact.GetValueOrDefault() || hasIgnoredTheory.GetValueOrDefault();
    }

    private void SetProperty(CodeTypeMember codeTypeMember, string name, string value)
    {
        _codeDomHelper.AddAttribute(codeTypeMember, TRAIT_ATTRIBUTE, name, value);
    }

    private void SetDescription(CodeTypeMember codeTypeMember, string description)
    {
        // xUnit doesn't have a DescriptionAttribute so using a TraitAttribute instead
        SetProperty(codeTypeMember, TRAIT_DESCRIPTION_ATTRIBUTE, description);
    }
}
