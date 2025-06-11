using Io.Cucumber.Messages.Types;
using System.Linq;

namespace Reqnroll.Formatters.RuntimeSupport
{
    public class PickleStepSequence
    {
        public bool HasPickles { get; }
        public Pickle CurrentPickle { get; }

        private int _PickleStepCounter;

        public PickleStepSequence(bool hasPickles, Pickle pickle)
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
