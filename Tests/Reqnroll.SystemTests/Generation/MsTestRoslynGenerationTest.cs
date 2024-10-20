using Microsoft.VisualStudio.TestTools.UnitTesting;
using Reqnroll.TestProjectGenerator;

namespace Reqnroll.SystemTests.Generation;

[TestClass]
[TestCategory("MsTest")]
[TestCategory("Roslyn")]
public class MsTestRoslynGenerationTest : MsTestGenerationTest
{
    protected override void TestInitialize()
    {
        base.TestInitialize();
        _testRunConfiguration.SourceGenerator = SourceGeneratorPlatform.Roslyn;
    }
}
