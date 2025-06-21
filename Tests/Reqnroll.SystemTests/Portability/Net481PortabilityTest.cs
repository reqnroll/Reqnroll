using Microsoft.VisualStudio.TestTools.UnitTesting;
using Reqnroll.TestProjectGenerator;
using Reqnroll.TestProjectGenerator.Data;

namespace Reqnroll.SystemTests.Portability;

[TestClass]
[TestCategory("NetFramework")]
[TestCategory("Net481")]
public class Net481PortabilityTest : PortabilityTestBase
{
    protected override void TestInitialize()
    {
        base.TestInitialize();
        _testRunConfiguration.TargetFramework = TargetFramework.Net481;
        _testRunConfiguration.ProgrammingLanguage = ProgrammingLanguage.CSharp73;
        SkipTUnit = true; // TUnit is not supported on .NET Framework 4.8.1
    }
}
