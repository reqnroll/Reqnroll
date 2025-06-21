using Microsoft.VisualStudio.TestTools.UnitTesting;
using Reqnroll.TestProjectGenerator;
using Reqnroll.TestProjectGenerator.Data;

namespace Reqnroll.SystemTests.Portability;

[TestClass]
[TestCategory("NetFramework")]
[TestCategory("Net472")]
public class Net472PortabilityTest : PortabilityTestBase
{
    protected override void TestInitialize()
    {
        base.TestInitialize();
        _testRunConfiguration.TargetFramework = TargetFramework.Net472;
        _testRunConfiguration.ProgrammingLanguage = ProgrammingLanguage.CSharp73;
        SkipTUnit = true; // TUnit is not supported on .NET Framework 4.7.2
    }
}
