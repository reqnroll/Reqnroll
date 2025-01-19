using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Reqnroll.TestProjectGenerator;

namespace Reqnroll.SystemTests.Generation;

/// <summary>
/// The purpose of these tests to verify that the tests generated by the
/// NUnit generator compile and can execute with NUnit.
/// </summary>
[TestClass]
[TestCategory("NUnit")]
public class NUnitGenerationTest : GenerationTestBase
{
    protected override void TestInitialize()
    {
        base.TestInitialize();
        _testRunConfiguration.UnitTestProvider = UnitTestProvider.NUnit3;
    }

    [TestMethod]
    public void GeneratorAllIn_sample_can_be_handled_by_NUnit4()
    {
        _testRunConfiguration.UnitTestProvider = UnitTestProvider.NUnit4;

        PrepareGeneratorAllInSamples();

        ExecuteTests();

        ShouldAllScenariosPass();

        ShouldFinishWithoutTestExecutionWarnings();
    }

    protected override void AssertIgnoredScenarioOutlineExampleHandled()
    {
        _vsTestExecutionDriver.LastTestExecutionResult.LeafTestResults
                              .Should().ContainSingle(tr => tr.TestName.StartsWith("SO") && tr.TestName.Contains("ignored"))
                              .Which.Outcome.Should().Be(GetExpectedIgnoredOutcome());
    }
}