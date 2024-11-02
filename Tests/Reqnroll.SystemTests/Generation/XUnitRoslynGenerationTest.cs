using Microsoft.VisualStudio.TestTools.UnitTesting;
using Reqnroll.TestProjectGenerator;

namespace Reqnroll.SystemTests.Generation;

[TestClass]
[TestCategory("xUnit")]
[TestCategory("Roslyn")]
public class XUnitRoslynGenerationTest : XUnitGenerationTest
{
    protected override void TestInitialize()
    {
        base.TestInitialize();
        _testRunConfiguration.SourceGenerator = SourceGeneratorPlatform.Roslyn;
    }
}
