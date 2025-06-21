using Microsoft.VisualStudio.TestTools.UnitTesting;
using Reqnroll.TestProjectGenerator;
using Reqnroll.TestProjectGenerator.Data;

namespace Reqnroll.SystemTests.Portability;

[TestClass]
[TestCategory("NetFramework")]
[TestCategory("Net462")]
public class Net462PortabilityTest : PortabilityTestBase
{
    protected override void TestInitialize()
    {
        base.TestInitialize();
        _testRunConfiguration.TargetFramework = TargetFramework.Net462;
        _testRunConfiguration.ProgrammingLanguage = ProgrammingLanguage.CSharp73;
        SkipTUnit = true; // TUnit is not supported on .NET Framework 4.6.2
    }
}