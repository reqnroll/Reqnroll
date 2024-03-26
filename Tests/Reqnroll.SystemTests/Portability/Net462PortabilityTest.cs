using Reqnroll.TestProjectGenerator;
using Reqnroll.TestProjectGenerator.Data;
using Xunit.Abstractions;

namespace Reqnroll.SystemTests.Portability;
public class Net462PortabilityTest : PortabilityTestBase
{
    public Net462PortabilityTest(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
    {
        _testRunConfiguration.TargetFramework = TargetFramework.Net462;
        _testRunConfiguration.ProgrammingLanguage = ProgrammingLanguage.CSharp73;
    }
}