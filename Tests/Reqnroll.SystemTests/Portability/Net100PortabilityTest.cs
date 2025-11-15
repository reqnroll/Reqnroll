using Microsoft.VisualStudio.TestTools.UnitTesting;
using Reqnroll.TestProjectGenerator.Data;

namespace Reqnroll.SystemTests.Portability;

[TestClass]
[TestCategory("DotNet")]
[TestCategory("Net100")]
public class Net100PortabilityTest : PortabilityTestBase
{
    protected override void TestInitialize()
    {
        base.TestInitialize();
        _testRunConfiguration.TargetFramework = TargetFramework.Net100;
    }
}
