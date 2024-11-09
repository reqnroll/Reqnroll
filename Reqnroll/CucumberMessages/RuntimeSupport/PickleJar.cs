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

        public bool HasPickles => Pickles != null && Pickles.Count() > 0;
        public IEnumerable<Pickle> Pickles { get; set; }

        public PickleJar(IEnumerable<Pickle> pickles)
        {
            Pickles = pickles;
            _PickleCounter = 0;
        }

        public PickleStepSequence PickleStepSequenceFor(string pickleIndex)
        {
            var pickleIndexInt = int.Parse(pickleIndex);
            if (HasPickles && (pickleIndexInt < 0 || pickleIndexInt >= Pickles.Count()))
                throw new ArgumentException("Invalid pickle index: " + pickleIndex);

            return new PickleStepSequence(HasPickles, HasPickles ? Pickles.ElementAt(pickleIndexInt): null);
        }
    }
}
