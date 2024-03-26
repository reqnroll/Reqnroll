using Reqnroll.TestProjectGenerator;
using Reqnroll.TestProjectGenerator.Data;
using Xunit.Abstractions;

namespace Reqnroll.SystemTests.Portability;

public class Net472PortabilityTest : PortabilityTestBase
{
    public Net472PortabilityTest(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
    {
        _testRunConfiguration.TargetFramework = TargetFramework.Net472;
        _testRunConfiguration.ProgrammingLanguage = ProgrammingLanguage.CSharp73;
    }
}
