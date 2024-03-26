using Reqnroll.TestProjectGenerator.Data;
using Xunit.Abstractions;

namespace Reqnroll.SystemTests.Portability;
public class Net60PortabilityTest : PortabilityTestBase
{
    public Net60PortabilityTest(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
    {
        _testRunConfiguration.TargetFramework = TargetFramework.Net60;
    }
}