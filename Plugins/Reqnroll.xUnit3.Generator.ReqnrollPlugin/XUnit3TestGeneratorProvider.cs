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

    const string IASYNCLIFETIME_INTERFACE = "Xunit.IAsyncLifetime";

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
            SetProperty(_currentFixtureDataTypeDeclaration, "Category", category);
        }
    }

    public void SetTestClassIgnore(TestClassGenerationContext generationContext)
    {
        throw new System.NotImplementedException();
    }

    public void FinalizeTestClass(TestClassGenerationContext generationContext)
    {
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
                    new CodeTypeReference("Xunit.ITestOutputHelper")),
                new CodeVariableReferenceExpression("_testOutputHelper")));
    }

    public void SetTestClassNonParallelizable(TestClassGenerationContext generationContext)
    {
        throw new System.NotImplementedException();
    }

    public void SetTestClassInitializeMethod(TestClassGenerationContext generationContext)
    {
        // ReSharper disable once BitwiseOperatorOnEnumWithoutFlags
        generationContext.TestClassInitializeMethod.Attributes |= MemberAttributes.Static;

        // ValueTask IAsyncLifetime.InitializeAsync() { <fixtureSetupMethod>(); }
        var initializeMethod = new CodeMemberMethod
        {
            PrivateImplementationType = new CodeTypeReference("Xunit.IAsyncLifetime"),
            Name = "InitializeAsync",
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
            PrivateImplementationType = new CodeTypeReference("System.IAsyncDisposable"),
            Name = "DisposeAsync",
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
        
        var asyncLifetimeInterface = new CodeTypeReference("Xunit.IAsyncLifetime");
        generationContext.TestClass.BaseTypes.Add(asyncLifetimeInterface);
            
        // ValueTask IAsyncLifetime.InitializeAsync() { <memberMethod>(); }
        var initializeMethod = new CodeMemberMethod
        {
            Attributes = MemberAttributes.Public,
            PrivateImplementationType = new CodeTypeReference("Xunit.IAsyncLifetime"),
            Name = "InitializeAsync",
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
            PrivateImplementationType = new CodeTypeReference("System.IAsyncDisposable"),
            Name = "DisposeAsync",
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
        _codeDomHelper.AddAttribute(testMethod, "Xunit.FactAttribute", new CodeAttributeArgument("DisplayName", new CodePrimitiveExpression(friendlyTestName)));
        SetProperty(testMethod, "FeatureTitle", generationContext.Feature.Name);
        SetDescription(testMethod, friendlyTestName);
    }

    public void SetTestMethodCategories(TestClassGenerationContext generationContext, CodeMemberMethod testMethod, IEnumerable<string> scenarioCategories)
    {
    }

    public void SetTestMethodIgnore(TestClassGenerationContext generationContext, CodeMemberMethod testMethod)
    {
        var factAttr = testMethod.CustomAttributes
                                 .OfType<CodeAttributeDeclaration>()
                                 .FirstOrDefault(codeAttributeDeclaration => codeAttributeDeclaration.Name == "Xunit.FactAttribute");

        // set [FactAttribute(Skip="reason")]
        factAttr?.Arguments.Add(new CodeAttributeArgument("Skip", new CodePrimitiveExpression("Ignored")));

        var theoryAttr = testMethod.CustomAttributes
                                   .OfType<CodeAttributeDeclaration>()
                                   .FirstOrDefault(codeAttributeDeclaration => codeAttributeDeclaration.Name == "Xunit.TheoryAttribute");

        // set [TheoryAttribute(Skip="reason")]
        theoryAttr?.Arguments.Add(new CodeAttributeArgument("Skip", new CodePrimitiveExpression("Ignored")));
    }

    public void SetRowTest(TestClassGenerationContext generationContext, CodeMemberMethod testMethod, string scenarioTitle)
    {
        _codeDomHelper.AddAttribute(testMethod, "Xunit.TheoryAttribute", new CodeAttributeArgument("DisplayName", new CodePrimitiveExpression(scenarioTitle)));
        SetProperty(testMethod, "FeatureTitle", generationContext.Feature.Name);
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

        _codeDomHelper.AddAttribute(testMethod, "Xunit.InlineDataAttribute", args.ToArray());
    }

    public void SetTestMethodAsRow(TestClassGenerationContext generationContext, CodeMemberMethod testMethod, string scenarioTitle, string exampleSetName, string variantName, IEnumerable<KeyValuePair<string, string>> arguments)
    {
        // doing nothing since we support RowTest
    }

    public void MarkCodeMethodInvokeExpressionAsAwait(CodeMethodInvokeExpression expression)
    {
        throw new System.NotImplementedException();
    }
    
    private CodeTypeReference CreateFixtureInterface(TestClassGenerationContext generationContext, CodeTypeReference fixtureDataType)
    {
        // Add a field for the ITestOutputHelper
        generationContext.TestClass.Members.Add(new CodeMemberField("Xunit.ITestOutputHelper", "_testOutputHelper"));

        // Store the fixture data type for later use in constructor
        generationContext.CustomData.Add("fixtureData", fixtureDataType);

        return new CodeTypeReference("Xunit.IClassFixture", fixtureDataType);
    }
    
    private void SetTestConstructor(TestClassGenerationContext generationContext, CodeConstructor constructor)
    {
        constructor.Parameters.Add(
            new CodeParameterDeclarationExpression((CodeTypeReference)generationContext.CustomData["fixtureData"], "fixtureData"));
        constructor.Parameters.Add(
            new CodeParameterDeclarationExpression("Xunit.ITestOutputHelper", "testOutputHelper"));

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
            "DisposeAsync");
        _codeDomHelper.MarkCodeMethodInvokeExpressionAsAwait(callDisposeException);

        method.Statements.Add(
            GetTryCatchStatementWithCombinedErrors(callTestInitializeMethodExpression, callDisposeException));
    }
    
    private void SetProperty(CodeTypeMember codeTypeMember, string name, string value)
    {
        _codeDomHelper.AddAttribute(codeTypeMember, "Xunit.TraitAttribute", name, value);
    }
    
    private void SetDescription(CodeTypeMember codeTypeMember, string description)
    {
        // xUnit doesn't have a DescriptionAttribute so using a TraitAttribute instead
        SetProperty(codeTypeMember, "Description", description);
    }
}
