using Microsoft.VisualStudio.TestTools.UnitTesting;
using Reqnroll.TestProjectGenerator;

namespace Reqnroll.SystemTests.Generation;

/// <summary>
/// The purpose of these tests to verify that the tests generated by the
/// xUnit generator compile and can execute with xUnit.
/// </summary>
[TestClass]
[TestCategory("xUnit3")]
public class XUnit3GenerationTest : GenerationTestBase
{
    protected override void TestInitialize()
    {
        base.TestInitialize();
        _testRunConfiguration.UnitTestProvider = UnitTestProvider.xUnit3;
    }
    
    protected override string GetExpectedPendingOutcome() => "Failed";
    
    protected override string GetExpectedUndefinedOutcome() => "Failed";

    protected override void AssertScenarioLevelParallelExecution()
    {
        //nop - xUnit currently does not support method-level parallelization
    }
}
