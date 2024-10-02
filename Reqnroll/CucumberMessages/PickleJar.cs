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

        public bool HasPickles { get; }
        public IEnumerable<Gherkin.CucumberMessages.Types.Pickle> Pickles { get; set; }

        //public PickleJar(IEnumerable<string> picklesJSON) : this(picklesJSON.Select(s => System.Text.Json.JsonSerializer.Deserialize<Gherkin.CucumberMessages.Types.Pickle>(s)).ToList())
        //{ }
        public PickleJar(string picklesJSON) : this(System.Text.Json.JsonSerializer.Deserialize<List<Gherkin.CucumberMessages.Types.Pickle>>(picklesJSON)) { }
        public PickleJar(IEnumerable<Gherkin.CucumberMessages.Types.Pickle> pickles) : this(pickles, 0, 0) { }

        public PickleJar(IEnumerable<Gherkin.CucumberMessages.Types.Pickle> pickles, int pickleCounter, int pickleStepCounter)
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
        public Gherkin.CucumberMessages.Types.Pickle CurrentPickle { get { return Pickles.ElementAt(_PickleCounter); } }

        public PickleStepSequence PickleStepSequenceFor(string pickleId)
        {
            return new PickleStepSequence(HasPickles, Pickles.Where(p => p.Id == pickleId).First());
        }

        public void NextPickle()
        {
            _PickleCounter++;
        }
    }


    public class PickleStepSequence
    {
        public bool HasPickles { get; }
        public Pickle CurrentPickle { get; }

        private int _PickleStepCounter;

        public PickleStepSequence(bool hasPickles, Gherkin.CucumberMessages.Types.Pickle pickle)
        {
            HasPickles = hasPickles;
            CurrentPickle = pickle;
            _PickleStepCounter = 0;
        }
        public void NextStep()
        {
            _PickleStepCounter++;
        }
        public string CurrentPickleStepId
        {
            get
            {
                if (!HasPickles) return null;
                return CurrentPickle.Steps.ElementAt(_PickleStepCounter).Id;
            }
        }

    }
}
