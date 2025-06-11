using Reqnroll.Time;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;

namespace Reqnroll.Formatters.ExecutionTracking
{
    /// <summary>
    /// Manages the lifecycle and lookup of <see cref="ITestCaseTracker"/> instances for a given feature.
    /// <para>
    /// <b>Responsibilities:</b>
    /// <list type="bullet">
    ///   <item>Creates and stores <see cref="ITestCaseTracker"/> objects, each representing a scenario (test case) within a feature.</item>
    ///   <item>Provides thread-safe access to trackers via a concurrent dictionary keyed by pickle (scenario) ID.</item>
    ///   <item>Supports retrieval and enumeration of all test case trackers for reporting or execution tracking.</item>
    /// </list>
    /// </para>
    /// <para>
    /// Typically, one <c>TestCaseTrackers</c> instance exists per <see cref="FeatureTracker"/>.
    /// </para>
    /// </summary>
    internal class TestCaseTrackers
    {
        internal ConcurrentDictionary<string, ITestCaseTracker> TestCaseTrackersById = new();
        internal IClock _clock;

        internal Func<FeatureTracker, string, ITestCaseTracker> TestCaseTrackerFactory; 
        private FeatureTracker _parentFeature;

        public TestCaseTrackers(FeatureTracker parentFeature, IClock clock, ICucumberMessageFactory messageFactory)
        {
            _parentFeature = parentFeature;
            _clock = clock;
            TestCaseTrackerFactory = (ft, pickleId) => { return new TestCaseTracker(pickleId, ft.TestRunStartedId, ft.FeatureName, ft.Enabled, ft.IDGenerator, ft.StepDefinitionsByMethodSignature, _clock.GetNowDateAndTime(), messageFactory); };

        }

        internal bool TryAddNew(string pickleId, out ITestCaseTracker testCaseTracker)
        {
            testCaseTracker = TestCaseTrackerFactory(_parentFeature, pickleId);
            return TestCaseTrackersById.TryAdd(pickleId, testCaseTracker);
        }

        internal bool TryGet(string pickleId, out ITestCaseTracker testCaseTracker)
        {
            testCaseTracker = null;
            return TestCaseTrackersById.TryGetValue(pickleId, out testCaseTracker);
        }

        internal ICollection<ITestCaseTracker> GetAll()
        {
            return TestCaseTrackersById.Values;
        }
    }
}
