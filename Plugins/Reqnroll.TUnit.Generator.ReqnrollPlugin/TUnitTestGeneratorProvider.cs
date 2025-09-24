using Reqnroll.BoDi;
using Reqnroll.Generator;
using Reqnroll.Generator.CodeDom;
using Reqnroll.Generator.UnitTestProvider;
using System.CodeDom;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace Reqnroll.TUnit.Generator.ReqnrollPlugin;

/// <summary>
/// A TUnit test generator provider.
/// This implementation leverages TUnit’s attribute-based model:
///   - [Test] marks test methods,
///   - [Before(Test)] / [After(Test)] for per-test hooks,
///   - [Before(Class)] / [After(Class)] for class-level setup/teardown,
///   - [Skip] to ignore tests,
///   - [Arguments] for row data,
///   - [DisplayName] for friendly names,
///   - [Category] for grouping, and
///   - [NotInParallel] to control parallel execution.
/// </summary>
[SuppressMessage("ReSharper", "BitwiseOperatorOnEnumWithoutFlags")]
public class TUnitTestGeneratorProvider : IUnitTestGeneratorProvider
{
    protected internal const string TEST_ATTR = "TUnit.Core.TestAttribute";
    protected internal const string BEFORE_ATTR = "TUnit.Core.BeforeAttribute";
    protected internal const string AFTER_ATTR = "TUnit.Core.AfterAttribute";
    protected internal const string SKIP_ATTR = "TUnit.Core.SkipAttribute";
    protected internal const string CATEGORY_ATTR = "TUnit.Core.CategoryAttribute";
    protected internal const string DISPLAYNAME_ATTR = "TUnit.Core.DisplayNameAttribute";
    protected internal const string ARGUMENTS_ATTR = "TUnit.Core.ArgumentsAttribute";
    protected internal const string NOTINPARALLEL_ATTR = "TUnit.Core.NotInParallelAttribute";
    protected internal const string TESTCONTEXT_TYPE = "TUnit.Core.TestContext";
    protected internal const string TESTCONTEXT_INSTANCE = "TUnit.Core.TestContext.Current";

    public TUnitTestGeneratorProvider(CodeDomHelper codeDomHelper)
    {
        CodeDomHelper = codeDomHelper;
    }

    protected CodeDomHelper CodeDomHelper { get; set; }

    public UnitTestGeneratorTraits GetTraits()
    {
        return UnitTestGeneratorTraits.RowTests | UnitTestGeneratorTraits.ParallelExecution;
    }

    public void SetTestClass(TestClassGenerationContext generationContext, string featureTitle, string featureDescription)
    {
    }

    public void SetTestClassCategories(TestClassGenerationContext generationContext, IEnumerable<string> featureCategories)
    {
        foreach (var category in featureCategories)
        {
            CodeDomHelper.AddAttribute(generationContext.TestClass, CATEGORY_ATTR, category);
        }
    }

    public void SetTestClassIgnore(TestClassGenerationContext generationContext)
    {
        CodeDomHelper.AddAttribute(generationContext.TestClass, SKIP_ATTR, "Ignored feature");
    }

    public void FinalizeTestClass(TestClassGenerationContext generationContext)
    {
        // testRunner.ScenarioContext.ScenarioContainer.RegisterInstanceAs<TUnit.Core.TestContext>(TUnit.Core.TestContext.Current);
        generationContext.ScenarioInitializeMethod.Statements.Add(
            new CodeMethodInvokeExpression(
                new CodeMethodReferenceExpression(
                    new CodePropertyReferenceExpression(
                        new CodePropertyReferenceExpression(
                            new CodeFieldReferenceExpression(null, generationContext.TestRunnerField.Name),
                            nameof(ScenarioContext)),
                        nameof(ScenarioContext.ScenarioContainer)),
                    nameof(IObjectContainer.RegisterInstanceAs),
                    new CodeTypeReference(TESTCONTEXT_TYPE)),
                GetTestContextExpression()));
    }

    private CodeExpression GetTestContextExpression() => new CodeVariableReferenceExpression(TESTCONTEXT_INSTANCE);


    public void SetTestClassNonParallelizable(TestClassGenerationContext generationContext)
    {
        CodeDomHelper.AddAttribute(generationContext.TestClass, NOTINPARALLEL_ATTR);
    }

    public void SetTestMethodNonParallelizable(TestClassGenerationContext generationContext, CodeMemberMethod testMethod)
    {
        CodeDomHelper.AddAttribute(testMethod, NOTINPARALLEL_ATTR);
    }

    public void SetTestClassInitializeMethod(TestClassGenerationContext generationContext)
    {
        // For class-level initialization, use [Before(Class)].
        CodeDomHelper.AddAttribute(
            generationContext.TestClassInitializeMethod,
            BEFORE_ATTR,
            new CodeAttributeArgument(
                new CodeFieldReferenceExpression(
                    new CodeTypeReferenceExpression("TUnit.Core.HookType"),
                    "Class")));
        generationContext.TestClassInitializeMethod.Attributes |= MemberAttributes.Static;
    }

    public void SetTestClassCleanupMethod(TestClassGenerationContext generationContext)
    {
        // For class-level cleanup, use [After(Class)].
        CodeDomHelper.AddAttribute(
            generationContext.TestClassCleanupMethod,
            AFTER_ATTR,
            new CodeAttributeArgument(
                new CodeFieldReferenceExpression(
                    new CodeTypeReferenceExpression("TUnit.Core.HookType"),
                    "Class")));
        generationContext.TestClassCleanupMethod.Attributes |= MemberAttributes.Static;

    }

    public void SetTestInitializeMethod(TestClassGenerationContext generationContext)
    {
        // For per-test initialization, use [Before(Test)].
        CodeDomHelper.AddAttribute(
            generationContext.TestInitializeMethod,
            BEFORE_ATTR,
            new CodeAttributeArgument(
                new CodeFieldReferenceExpression(
                    new CodeTypeReferenceExpression("TUnit.Core.HookType"),
                    "Test")));
    }

    public void SetTestCleanupMethod(TestClassGenerationContext generationContext)
    {
        // For per-test cleanup, use [After(Test)].
        CodeDomHelper.AddAttribute(
            generationContext.TestCleanupMethod,
            AFTER_ATTR,
            new CodeAttributeArgument(
                new CodeFieldReferenceExpression(
                    new CodeTypeReferenceExpression("TUnit.Core.HookType"),
                    "Test")));

    }

    public void SetTestMethod(TestClassGenerationContext generationContext, CodeMemberMethod testMethod, string friendlyTestName)
    {
        // Mark the method as a test and add a friendly name.
        CodeDomHelper.AddAttribute(testMethod, TEST_ATTR);
        CodeDomHelper.AddAttribute(testMethod, DISPLAYNAME_ATTR, friendlyTestName);
    }

    public void SetTestMethodCategories(TestClassGenerationContext generationContext, CodeMemberMethod testMethod, IEnumerable<string> scenarioCategories)
    {
        foreach (var category in scenarioCategories)
        {
            CodeDomHelper.AddAttribute(testMethod, CATEGORY_ATTR, category);
        }
    }

    public void SetTestMethodIgnore(TestClassGenerationContext generationContext, CodeMemberMethod testMethod)
    {
        // Ignore an individual test.
        CodeDomHelper.AddAttribute(testMethod, SKIP_ATTR, "Ignored scenario");
    }

    public void SetRowTest(TestClassGenerationContext generationContext, CodeMemberMethod testMethod, string scenarioTitle)
    {
        // For a row test, mark it as a test with a display name.
        var paramNames = string.Join(", ", testMethod.Parameters.Cast<CodeParameterDeclarationExpression>().Take(testMethod.Parameters.Count - 2).Select(x => $"${x.Name}"));
        SetTestMethod(generationContext, testMethod, scenarioTitle + " (" + paramNames + ")");
    }

    public void SetRow(TestClassGenerationContext generationContext, CodeMemberMethod testMethod, IEnumerable<string> arguments, IEnumerable<string> tags, bool isIgnored)
    {
        var args = arguments.Select(
            arg => new CodeAttributeArgument(new CodePrimitiveExpression(arg))).ToList();

        var tagsArray = tags.ToList();
        if (isIgnored)
            tagsArray.Add("ignore");

        // addressing ReSharper bug: TestCase attribute with empty string[] param causes inconclusive result - https://youtrack.jetbrains.com/issue/RSRP-279138
        var exampleTagExpressionList = tagsArray.Select(t => (CodeExpression)new CodePrimitiveExpression(t));
        var exampleTagsExpression = new CodeArrayCreateExpression(typeof(string[]), exampleTagExpressionList.ToArray());

        args.Add(new CodeAttributeArgument(exampleTagsExpression));

        CodeDomHelper.AddAttribute(testMethod, ARGUMENTS_ATTR, args.ToArray());
    }

    public void SetTestMethodAsRow(TestClassGenerationContext generationContext, CodeMemberMethod testMethod, string scenarioTitle, string exampleSetName, string variantName, IEnumerable<KeyValuePair<string, string>> arguments)
    {
    }

    public void MarkCodeMethodInvokeExpressionAsAwait(CodeMethodInvokeExpression expression)
    {
        CodeDomHelper.MarkCodeMethodInvokeExpressionAsAwait(expression);
    }
}
