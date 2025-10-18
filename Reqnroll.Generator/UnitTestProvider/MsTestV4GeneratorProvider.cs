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

    public override void SetTestMethod(TestClassGenerationContext generationContext, CodeMemberMethod testMethod, string friendlyTestName)
    {
        if (generationContext.DisableFriendlyTestNames)
        {
            CodeDomHelper.AddAttribute(testMethod, TEST_ATTR);
        }
        else
        {
            CodeDomHelper.AddAttribute(testMethod, TEST_ATTR, new CodeAttributeArgument("DisplayName", new CodePrimitiveExpression(friendlyTestName)));
        }

        CodeDomHelper.AddAttribute(testMethod, DESCRIPTION_ATTR, friendlyTestName);

        //as in mstest, you cannot mark classes with the description attribute, we
        //just apply it for each test method as a property
        SetProperty(testMethod, FEATURE_TITILE_PROPERTY_NAME, generationContext.Feature.Name);
    }
}
