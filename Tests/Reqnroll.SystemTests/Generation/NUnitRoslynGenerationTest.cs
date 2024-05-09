using Microsoft.VisualStudio.TestTools.UnitTesting;
using Reqnroll.TestProjectGenerator;

namespace Reqnroll.SystemTests.Generation;

[TestClass]
[TestCategory("NUnit")]
[TestCategory("Roslyn")]
public class NUnitRoslynGenerationTest : NUnitGenerationTest
{
    protected override void TestInitialize()
    {
        base.TestInitialize();
        _testRunConfiguration.SourceGenerator = SourceGeneratorPlatform.Roslyn;
    }
}
