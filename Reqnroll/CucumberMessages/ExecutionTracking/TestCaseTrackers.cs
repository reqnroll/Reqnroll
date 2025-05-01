using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;

namespace Reqnroll.CucumberMessages.ExecutionTracking
{
    internal class TestCaseTrackers
    {
        internal ConcurrentDictionary<string, ITestCaseTracker> TestCaseTrackersById = new();

        internal Func<FeatureTracker, string, ITestCaseTracker> TestCaseTrackerFactory = 
            (ft, pickleId) => { return new TestCaseTracker(pickleId, ft.TestRunStartedId, ft.FeatureName, ft.Enabled, ft.IDGenerator, ft.StepDefinitionsByPattern); };
        private FeatureTracker _parentFeature;

        public TestCaseTrackers(FeatureTracker parentFeature)
        {
            _parentFeature = parentFeature;
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
