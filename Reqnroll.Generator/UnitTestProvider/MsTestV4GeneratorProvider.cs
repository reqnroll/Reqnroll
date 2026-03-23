using Reqnroll.Generator.CodeDom;
using System.CodeDom;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace Reqnroll.Generator.UnitTestProvider;

[SuppressMessage("ReSharper", "BitwiseOperatorOnEnumWithoutFlags")]
public class MsTestV4GeneratorProvider(CodeDomHelper codeDomHelper) : MsTestV2GeneratorProvider(codeDomHelper)
{
    public override void SetTestClassCleanupMethod(TestClassGenerationContext generationContext)
    {
        generationContext.TestClassCleanupMethod.Attributes |= MemberAttributes.Static;
        // V4 - eliminates the ClassCleanupBehavior enum argument
        // [Microsoft.VisualStudio.TestTools.UnitTesting.ClassCleanupAttribute()]
        CodeDomHelper.AddAttribute(generationContext.TestClassCleanupMethod, TESTFIXTURETEARDOWN_ATTR);
    }

    public override void AddTestMethodAttribute(TestClassGenerationContext generationContext, CodeMemberMethod testMethod, string friendlyTestName)
    {
        // V4 - the DisplayName property must be explicitly set

        CodeAttributeDeclaration testAttribute;
        if (generationContext.DisableFriendlyTestNames)
        {
            testAttribute = CodeDomHelper.AddAttribute(testMethod, TEST_ATTR);
        }
        else
        {
            testAttribute = CodeDomHelper.AddAttribute(testMethod, TEST_ATTR, new CodeAttributeArgument("DisplayName", new CodePrimitiveExpression(friendlyTestName)));
        }

        // we only support line number in C#
        if (CodeDomHelper.TargetLanguage == CodeDomProviderLanguage.CSharp)
            testAttribute.Arguments.Insert(0, new CodeAttributeArgument(new CodeSnippetExpression($"callerLineNumber: {generationContext.CurrentScenarioDefinition.ScenarioDefinition.Location.Line}")));
    }

    protected override string FindDisplayNameFromTestMethodAttribute(CodeAttributeDeclaration testMethodAttr)
    {
        // Find the DisplayName argument value
        string testMethodDisplayName = null;
        if (testMethodAttr != null && testMethodAttr.Arguments.Count >= 1)
        {
            var displayNameArg = testMethodAttr.Arguments
                .OfType<CodeAttributeArgument>()
                .Where(arg => arg.Name == "DisplayName")
                .FirstOrDefault();
            if (displayNameArg != null && displayNameArg.Value is CodePrimitiveExpression expr && expr.Value is string str)
            {
                testMethodDisplayName = str;
            }
        }

        return testMethodDisplayName;
    }

}
