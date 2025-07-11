using Reqnroll;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CucumberMessages.CompatibilityTests.Samples.ExternalBindingAssemblies
{
    [Binding]
    public class SampleExternalBindingClass
    {
        [When("the sample external binding class is called")]
        public void WhenTheSampleExternalBindingClassIsCalled()
        {
            // nop
        }
    }
}
