using Reqnroll.Generator.CodeDom;
using Reqnroll.Generator.UnitTestProvider;

namespace Reqnroll.GeneratorTests.UnitTestProvider
{
    public class MsTestV4GeneratorProviderTests : MsTestGeneratorProviderTestsBase
    {
        protected override MsTestGeneratorProvider CreateProvider(CodeDomHelper codeDomHelper)
        {
            return new MsTestV4GeneratorProvider(codeDomHelper);
        }

        protected override string ExpectedDoNotParallelizeAttributeName => 
            MsTestV4GeneratorProvider.DONOTPARALLELIZE_ATTR;

        protected override string ExpectedPriorityAttributeName => 
            MsTestV4GeneratorProvider.PRIORITY_ATTR;
    }
}