using Io.Cucumber.Messages.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Reqnroll.Formatters.RuntimeSupport
{
    /// <summary>
    /// This class is used at runtime to provide the appropriate PickleId and PickleStepId to the TestCaseTracker and StepTracker.
    /// </summary>
    internal class PickleJar
    {
        private bool HasPickles => Pickles != null && Pickles.Count() > 0;
        private IEnumerable<Pickle> Pickles;

        internal PickleJar(IEnumerable<Pickle> pickles)
        {
            Pickles = pickles;
        }

        internal PickleStepSequence PickleStepSequenceFor(string pickleIndex)
        {
            var pickleIndexInt = int.Parse(pickleIndex);
            if (HasPickles && (pickleIndexInt < 0 || pickleIndexInt >= Pickles.Count()))
                throw new ArgumentException("Invalid pickle index: " + pickleIndex);

            return new PickleStepSequence(HasPickles, HasPickles ? Pickles.ElementAt(pickleIndexInt) : null);
        }
    }
}
