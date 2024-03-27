using Reqnroll.TestProjectGenerator;
using Reqnroll.TestProjectGenerator.Driver;

namespace Reqnroll.SystemTests.Drivers;

//ported from Specs
public class ExecutionDriver(
    VSTestExecutionDriver _vsTestExecutionDriver, 
    CompilationDriver _compilationDriver, 
    TestRunConfiguration _testRunConfiguration)
{
    public void ExecuteTestsWithTag(string tag)
    {
        _vsTestExecutionDriver.Filter = _testRunConfiguration.UnitTestProvider == UnitTestProvider.xUnit ? 
            $"Category={tag}" : 
            $"TestCategory={tag}";

        ExecuteTests();
    }

    public void ExecuteTests()
    {
        ExecuteTestsTimes(1);
    }

    public void ExecuteTestsTimes(uint times)
    {
        if (!_compilationDriver.HasTriedToCompile)
        {
            _compilationDriver.CompileSolution();
        }

        for (uint currentTime = 0; currentTime < times; currentTime++)
        {
            _vsTestExecutionDriver.ExecuteTests();
        }
    }
}