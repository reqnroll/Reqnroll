using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace Reqnroll.CucumberMesssages
{
    internal class FeatureState
    {
        public string Name { get; set; }
        public bool Enabled { get; set; } //This will be false if the feature could not be pickled

        // These two flags are used to avoid duplicate messages being sent when Scenarios within Features are run concurrently
        // and multiple FeatureStartedEvent and FeatureFinishedEvent events are fired
        public bool Started { get; set; }
        public bool Finished { get; set; }

        public Dictionary<string, string> ScenarioNameIDMap = new Dictionary<string, string>(); // <scenarioName, scenarioID>
        public Dictionary<string, string> StepPatternIDMap = new Dictionary<string, string>(); // <stepPattern, stepID>

        public ConcurrentQueue<ReqnrollCucumberMessage> Messages = new ConcurrentQueue<ReqnrollCucumberMessage>();
        public ConcurrentStack<int> workerThreadMarkers = new ConcurrentStack<int>();
    }
}