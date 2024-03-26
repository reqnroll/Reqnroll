using Reqnroll.TestProjectGenerator.Data;
using Xunit.Abstractions;

namespace Reqnroll.SystemTests.Portability;

public class Net70PortabilityTest : PortabilityTestBase
{
    public Net70PortabilityTest(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
    {
        _testRunConfiguration.TargetFramework = TargetFramework.Net70;
    }
}
