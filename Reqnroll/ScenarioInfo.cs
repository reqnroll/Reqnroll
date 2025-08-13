using Reqnroll.Formatters.RuntimeSupport;
using System;
using System.Collections.Specialized;
using System.Linq;

namespace Reqnroll
{
    /// <summary>
    /// Contains information about the scenario currently being executed.
    /// </summary>
    public class ScenarioInfo
    {
        /// <summary>
        /// Direct tags of the scenario, including tags of the examples block.
        /// </summary>
        public string[] Tags { get; }
        /// <summary>
        /// Contains direct tags and tags inherited from the feature and the rule.
        /// </summary>
        public string[] CombinedTags { get; private set; }

        /// <summary>
        /// The arguments used to execute a scenario outline example.
        /// </summary>
        public IOrderedDictionary Arguments { get; }

        /// <summary>
        /// The title (name) of the scenario.
        /// </summary>
        public string Title { get; }

        /// <summary>
        /// The description of the scenario.
        /// </summary>
        public string Description { get; }

        /// <summary>
        /// The PickleIdIndex of the test scenario when exported as a Cucumber Messages "pickle". Used internally.
        /// The index is the sequential number of the pickle in the list of pickles generated from the feature file.
        /// The index is "replaced" by real unique identifiers (<see cref="PickleId"/>) during <see cref="Events.ScenarioStartedEvent"/> event.
        /// </summary>
        internal string PickleIdIndex { get; }

        /// <summary>
        /// The list of step PickleIds in the step sequence for this test. Used internally at runtime.
        /// </summary>
        internal PickleStepSequence PickleStepSequence { get; set; }

        /// <summary>
        /// This holds the unique identifier for the test ("pickle") represented by this scenario info. Used internally at runtime.
        /// </summary>
        internal string PickleId { get; set; }


        public ScenarioInfo(string title, string description, string[] tags, IOrderedDictionary arguments, string[] inheritedTags = null, string pickleIndex = null)
        {
            Title = title;
            Description = description;
            Tags = tags ?? Array.Empty<string>();
            Arguments = arguments;
            CombinedTags = Tags.Concat(inheritedTags ?? Array.Empty<string>()).ToArray();
            PickleIdIndex = pickleIndex;
        }
    }
}