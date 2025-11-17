using Reqnroll.Generator.CodeDom;
using System.CodeDom;
using System.Diagnostics.CodeAnalysis;

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

        if (generationContext.DisableFriendlyTestNames)
        {
            CodeDomHelper.AddAttribute(testMethod, TEST_ATTR);
        }
        else
        {
            CodeDomHelper.AddAttribute(testMethod, TEST_ATTR, new CodeAttributeArgument("DisplayName", new CodePrimitiveExpression(friendlyTestName)));
        }
    }
}
