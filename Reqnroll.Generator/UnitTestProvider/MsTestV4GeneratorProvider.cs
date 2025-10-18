using Reqnroll.Generator.CodeDom;
using System.CodeDom;

namespace Reqnroll.Generator.UnitTestProvider
{
    public class MsTestV4GeneratorProvider : MsTestV2GeneratorProvider
    {
        public MsTestV4GeneratorProvider(CodeDomHelper codeDomHelper) : base(codeDomHelper)
        {
        }

        public override void SetTestClassCleanupMethod(TestClassGenerationContext generationContext)
        {
            generationContext.TestClassCleanupMethod.Attributes |= MemberAttributes.Static;
            // [Microsoft.VisualStudio.TestTools.UnitTesting.ClassCleanupAttribute]
            CodeDomHelper.AddAttribute(generationContext.TestClassCleanupMethod, TESTFIXTURETEARDOWN_ATTR);
        }
    }
}
