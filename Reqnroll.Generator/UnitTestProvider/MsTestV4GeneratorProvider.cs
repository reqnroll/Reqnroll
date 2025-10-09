using Reqnroll.Generator.CodeDom;
using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Text;

namespace Reqnroll.Generator.UnitTestProvider;

public class MsTestV4GeneratorProvider : MsTestV2GeneratorProvider
{
    public MsTestV4GeneratorProvider(CodeDomHelper codeDomHelper) : base(codeDomHelper)
    {
    }
    public override void SetTestClassCleanupMethod(TestClassGenerationContext generationContext)
    {
        generationContext.TestClassCleanupMethod.Attributes |= MemberAttributes.Static;
        // V4 - eliminates the ClassCleanupBehavior enum argument
        // [Microsoft.VisualStudio.TestTools.UnitTesting.ClassCleanupAttribute()]
        var attribute = CodeDomHelper.AddAttribute(generationContext.TestClassCleanupMethod, TESTFIXTURETEARDOWN_ATTR);
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
        base.SetProperty(testMethod, FEATURE_TITILE_PROPERTY_NAME, generationContext.Feature.Name);
    }

}
