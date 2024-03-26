using Reqnroll.TestProjectGenerator;
using Reqnroll.TestProjectGenerator.Data;
using Xunit.Abstractions;

namespace Reqnroll.SystemTests.Portability;

public class Net481PortabilityTest : PortabilityTestBase
{
    public Net481PortabilityTest(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
    {
        _testRunConfiguration.TargetFramework = TargetFramework.Net481;
        _testRunConfiguration.ProgrammingLanguage = ProgrammingLanguage.CSharp73;
    }
}
