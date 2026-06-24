using System.Linq;
using FluentAssertions;
using Gherkin.CucumberMessages;
using Moq;
using Reqnroll.Formatters.ExecutionTracking;
using Xunit;

namespace Reqnroll.RuntimeTests.Formatters.ExecutionTracking;

public class TestCaseTrackerTests
{
    private readonly Mock<IIdGenerator> _idGeneratorMock;
    private readonly TestCaseTracker _sut;
    private int _idCounter;

    public TestCaseTrackerTests()
    {
        _idCounter = 0;
        _idGeneratorMock = new Mock<IIdGenerator>();
        _idGeneratorMock.Setup(g => g.GetNewId()).Returns(() => $"id-{++_idCounter}");

        var pickleTrackerMock = new Mock<IPickleExecutionTracker>();
        pickleTrackerMock.SetupGet(p => p.IdGenerator).Returns(_idGeneratorMock.Object);

        _sut = new TestCaseTracker("testCaseId", "pickleId", pickleTrackerMock.Object);
    }

    [Fact]
    public void GetOrCreateTestStepTracker_SamePickleIdAndOccurrence_ReturnsSameInstance()
    {
        // Simulates the same step being looked up on attempt 0 then again on attempt 1.
        // Both attempts produce occurrence=1 for ps1, so the same ledger entry is reused.
        var first = _sut.GetOrCreateTestStepTracker("ps1", 1);
        var second = _sut.GetOrCreateTestStepTracker("ps1", 1);

        first.Should().BeSameAs(second);
        _sut.Steps.Should().HaveCount(1);
    }

    [Fact]
    public void GetOrCreateTestStepTracker_TruncatedAttempt0_StepOnlySeenOnRetry_AppendsNewEntry()
    {
        // Attempt 0 is truncated: only ps1 fires (occurrence=1). ps2 is never reached.
        var ps1Attempt0 = _sut.GetOrCreateTestStepTracker("ps1", 1);

        // Attempt 1 (retry): ps1 fires again (finds existing entry), then ps2 fires for the first time.
        var ps1Retry = _sut.GetOrCreateTestStepTracker("ps1", 1);
        var ps2Retry = _sut.GetOrCreateTestStepTracker("ps2", 1);

        ps1Attempt0.Should().BeSameAs(ps1Retry);
        ps2Retry.Should().NotBeSameAs(ps1Attempt0);
        _sut.Steps.Should().HaveCount(2);
        _sut.Steps.OfType<TestStepTracker>().Should().Contain(t => t.PickleStepId == "ps2");
    }

    [Fact]
    public void GetOrCreateHookStepTracker_SameHookFiredMultipleTimes_CreatesOneEntryPerOccurrence()
    {
        // BeforeStep fires once per step; three steps yield three distinct occurrences of the same hook.
        var occ1 = _sut.GetOrCreateHookStepTracker("hook1", 1);
        var occ2 = _sut.GetOrCreateHookStepTracker("hook1", 2);
        var occ3 = _sut.GetOrCreateHookStepTracker("hook1", 3);

        occ1.Should().NotBeSameAs(occ2);
        occ2.Should().NotBeSameAs(occ3);
        _sut.Steps.Should().HaveCount(3);
    }

    [Fact]
    public void GetOrCreateHookStepTracker_SameOccurrenceOnRetry_ReturnsSameInstance()
    {
        // Simulates the same hook firing as occurrence=1 on attempt 0 and again on attempt 1.
        // The ledger entry must be stable across retries.
        var first = _sut.GetOrCreateHookStepTracker("hook1", 1);
        var second = _sut.GetOrCreateHookStepTracker("hook1", 1);

        first.Should().BeSameAs(second);
        _sut.Steps.Should().HaveCount(1);
    }
}
