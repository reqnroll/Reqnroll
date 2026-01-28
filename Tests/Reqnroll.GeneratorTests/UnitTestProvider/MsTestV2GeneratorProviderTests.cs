using FluentAssertions;
using Reqnroll.Generator.CodeDom;
using Reqnroll.Generator.UnitTestProvider;
using Reqnroll.Parser;
using System.CodeDom;
using System.Globalization;
using System.IO;
using System.Linq;
using Xunit;

namespace Reqnroll.GeneratorTests.UnitTestProvider
{
    public class MsTestV2GeneratorProviderTests : MsTestGeneratorProviderTestsBase
    {
        protected override MsTestGeneratorProvider CreateProvider(CodeDomHelper codeDomHelper)
        {
            return new MsTestV2GeneratorProvider(codeDomHelper);
        }

        protected override string ExpectedDoNotParallelizeAttributeName => 
            MsTestV2GeneratorProvider.DONOTPARALLELIZE_ATTR;

        protected override string ExpectedPriorityAttributeName => 
            MsTestV2GeneratorProvider.PRIORITY_ATTR;
    }
}
