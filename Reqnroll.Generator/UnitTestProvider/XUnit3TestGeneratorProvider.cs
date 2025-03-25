using System.CodeDom;
using System.Collections.Generic;
using Reqnroll.BoDi;
using Reqnroll.Generator.CodeDom;

namespace Reqnroll.Generator.UnitTestProvider;

public sealed class XUnit3TestGeneratorProvider(CodeDomHelper codeDomHelper)
    : IUnitTestGeneratorProvider
{
    private CodeTypeDeclaration _currentFixtureDataTypeDeclaration = null;
    private readonly CodeTypeReference _objectCodeTypeReference = new(typeof(object));
    private readonly CodeDomHelper _codeDomHelper = codeDomHelper;

    public UnitTestGeneratorTraits GetTraits() => UnitTestGeneratorTraits.None;

    public void SetTestClass(TestClassGenerationContext generationContext, string featureTitle, string featureDescription)
    {
        _currentFixtureDataTypeDeclaration = _codeDomHelper.CreateGeneratedTypeDeclaration("FixtureData");
        
        // have to add the explicit object base class because of VB.NET
        _currentFixtureDataTypeDeclaration.BaseTypes.Add(typeof(object));
        var asyncLifetimeInterface = new CodeTypeReference("Xunit.IAsyncLifetime");
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
        throw new System.NotImplementedException();
    }

    public void SetTestClassIgnore(TestClassGenerationContext generationContext)
    {
        throw new System.NotImplementedException();
    }

    public void FinalizeTestClass(TestClassGenerationContext generationContext)
    {
    }

    public void SetTestClassNonParallelizable(TestClassGenerationContext generationContext)
    {
        throw new System.NotImplementedException();
    }

    public void SetTestClassInitializeMethod(TestClassGenerationContext generationContext)
    {
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

        _codeDomHelper.MarkCodeMethodInvokeExpressionAsAwait(expression);

        initializeMethod.Statements.Add(expression);
    }

    public void SetTestClassCleanupMethod(TestClassGenerationContext generationContext)
    {
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

        _codeDomHelper.MarkCodeMethodInvokeExpressionAsAwait(expression);

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

        _codeDomHelper.MarkCodeMethodInvokeExpressionAsAwait(expression);

        disposeMethod.Statements.Add(expression);
    }

    public void SetTestMethod(TestClassGenerationContext generationContext, CodeMemberMethod testMethod, string friendlyTestName)
    {
        _codeDomHelper.AddAttribute(testMethod, "Xunit.FactAttribute", new CodeAttributeArgument("DisplayName", new CodePrimitiveExpression(friendlyTestName)));
    }

    public void SetTestMethodCategories(TestClassGenerationContext generationContext, CodeMemberMethod testMethod, IEnumerable<string> scenarioCategories)
    {
    }

    public void SetTestMethodIgnore(TestClassGenerationContext generationContext, CodeMemberMethod testMethod)
    {
        throw new System.NotImplementedException();
    }

    public void SetRowTest(TestClassGenerationContext generationContext, CodeMemberMethod testMethod, string scenarioTitle)
    {
        throw new System.NotImplementedException();
    }

    public void SetRow(TestClassGenerationContext generationContext, CodeMemberMethod testMethod, IEnumerable<string> arguments, IEnumerable<string> tags, bool isIgnored)
    {
        throw new System.NotImplementedException();
    }

    public void SetTestMethodAsRow(TestClassGenerationContext generationContext, CodeMemberMethod testMethod, string scenarioTitle, string exampleSetName, string variantName, IEnumerable<KeyValuePair<string, string>> arguments)
    {
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
    
    private void SetTestInitializeMethod(TestClassGenerationContext generationContext, CodeMemberMethod method)
    {
        var callTestInitializeMethodExpression = new CodeMethodInvokeExpression(
            new CodeThisReferenceExpression(),
            generationContext.TestInitializeMethod.Name);

        _codeDomHelper.MarkCodeMethodInvokeExpressionAsAwait(callTestInitializeMethodExpression);

        method.Statements.Add(callTestInitializeMethodExpression);
    }
}
