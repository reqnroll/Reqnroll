using Reqnroll;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CucumberMessages.CompatibilityTests.CCK.pending
{
    [Binding]
    internal class Pending
    {
        [Given("an implemented non-pending step")]
        public void GivenAnImplementedNonPendingStep()
        {
            //nop
        }

        [Given("an implemented step that is skipped")]
        public void GivenAnImplementedStepThatIsSkipped()
        {
            throw new ApplicationException("This step should not have been executed");
        }

        [Given("an unimplemented pending step")]
        public void GivenAnUnimplementedPendingStep()
        {
            throw new PendingStepException();
        }
    }
}
