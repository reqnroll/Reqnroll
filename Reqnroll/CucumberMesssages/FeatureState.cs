using Gherkin.CucumberMessages;
using System;
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

        //ID Generator to use when generating IDs for TestCase messages and beyond
        // If gherkin feature was generated using integer IDs, then we will use an integer ID generator seeded with the last known integer ID
        // otherwise we'll use a GUID ID generator
        public IIdGenerator IDGenerator { get; set; }

        //Lookup tables
        public Dictionary<string, string> StepDefinitionsByPattern = new();
        public Dictionary<string, Io.Cucumber.Messages.Types.Pickle> PicklesByScenarioName = new();

        public Dictionary<string, ScenarioState> ScenarioName2StateMap = new(); 

        public ConcurrentQueue<ReqnrollCucumberMessage> Messages = new();
        public ConcurrentStack<int> workerThreadMarkers = new();
    }
}