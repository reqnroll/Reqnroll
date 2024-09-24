using Gherkin.CucumberMessages.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Reqnroll.CucumberMessages
{
    public class PickleJar
    {
        public const string PICKLEJAR_VARIABLE_NAME = "m_pickleJar";

        public int _PickleCounter = 0;
        public IEnumerable<Gherkin.CucumberMessages.Types.Pickle> Pickles { get; set; }

        //public PickleJar(IEnumerable<string> picklesJSON) : this(picklesJSON.Select(s => System.Text.Json.JsonSerializer.Deserialize<Gherkin.CucumberMessages.Types.Pickle>(s)).ToList())
        //{ }
        public PickleJar(string picklesJSON) : this(System.Text.Json.JsonSerializer.Deserialize<List<Gherkin.CucumberMessages.Types.Pickle>>(picklesJSON)) { }
        public PickleJar(IEnumerable<Gherkin.CucumberMessages.Types.Pickle> pickles) : this(pickles, 0, 0) { }

        public PickleJar(IEnumerable<Gherkin.CucumberMessages.Types.Pickle> pickles, int pickleCounter, int pickleStepCounter)
        {
            Pickles = pickles;
            _PickleCounter = pickleCounter;
        }

        public string CurrentPickleId { get { return Pickles.ElementAt(_PickleCounter).Id; } }
        public Gherkin.CucumberMessages.Types.Pickle CurrentPickle { get { return Pickles.ElementAt(_PickleCounter); } }

        public IEnumerable<string> PickleStepIdsFor(string pickleId)
        {
            return Pickles.Where(p => p.Id == pickleId).SelectMany(p => p.Steps.Select(s => s.Id)).ToArray();
        }
        public PickleStepSequence PickleStepSequenceFor(string pickleId)
        {
            return new PickleStepSequence(Pickles.Where(p => p.Id == pickleId).First());
        }

        public void NextPickle()
        {
            _PickleCounter++;
        }
    }


    public class PickleStepSequence
    {
        public Pickle CurrentPickle { get; }

        private int _PickleStepCounter;

        public PickleStepSequence(Gherkin.CucumberMessages.Types.Pickle pickle)
        {
            CurrentPickle = pickle;
            _PickleStepCounter = 0;
        }
        public void NextStep()
        {
            _PickleStepCounter++;
        }
        public string CurrentPickleStepId { get { return CurrentPickle.Steps.ElementAt(_PickleStepCounter).Id; } }

    }
}
