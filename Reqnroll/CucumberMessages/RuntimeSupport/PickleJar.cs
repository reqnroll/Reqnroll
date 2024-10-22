using Io.Cucumber.Messages.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Reqnroll.CucumberMessages.RuntimeSupport
{
    /// <summary>
    /// These classes are used at runtime to track which Pickle and PickleStep is being executed
    /// and to provide the appropriate PickleId and PickleStepId to the TestCaseTracker and StepTracker.
    /// </summary>
    public class PickleJar
    {
        public int _PickleCounter = 0;

        public bool HasPickles { get; }
        public IEnumerable<Pickle> Pickles { get; set; }

        public PickleJar(string picklesJSON) : this(System.Text.Json.JsonSerializer.Deserialize<List<Pickle>>(picklesJSON)) { }
        public PickleJar(IEnumerable<Pickle> pickles) : this(pickles, 0, 0) { }

        public PickleJar(IEnumerable<Pickle> pickles, int pickleCounter, int pickleStepCounter)
        {
            Pickles = pickles;
            _PickleCounter = pickleCounter;
            HasPickles = pickles != null && pickles.Count() > 0;
        }

        public string CurrentPickleId
        {
            get
            {
                if (!HasPickles) return null;
                return Pickles.ElementAt(_PickleCounter).Id;
            }
        }
        public Pickle CurrentPickle { get { return Pickles.ElementAt(_PickleCounter); } }

        public PickleStepSequence PickleStepSequenceFor(string pickleIndex)
        {
            return new PickleStepSequence(HasPickles, HasPickles ? Pickles.ElementAt(int.Parse(pickleIndex)): null);
        }

        public void NextPickle()
        {
            _PickleCounter++;
        }
    }
}
