using Reqnroll;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CucumberMessages.CompatibilityTests.Samples.ExternalBindingAssemblies
{
    [Binding]
    public class SampleInternalBindingClass
    {
        [Given("I have {int} cukes in my belly")]
        public void GivenIHaveCukesInMyBelly(int cukes)
        {
            // nop
        }
    }
}
