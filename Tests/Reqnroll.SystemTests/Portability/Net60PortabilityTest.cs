using Microsoft.VisualStudio.TestTools.UnitTesting;
using Reqnroll.TestProjectGenerator.Data;

namespace Reqnroll.SystemTests.Portability;

[TestClass]
[TestCategory("DotNet")]
[TestCategory("Net60")]
public class Net60PortabilityTest : PortabilityTestBase
{
    protected override void TestInitialize()
    {
        base.TestInitialize();
        _testRunConfiguration.TargetFramework = TargetFramework.Net60;
        SkipTUnit = true; // TUnit is not supported on .NET 6
    }
}