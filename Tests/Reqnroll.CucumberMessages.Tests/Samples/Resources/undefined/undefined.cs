using Reqnroll;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CucumberMessages.CompatibilityTests.CCK.undefined
{
    [Binding]
    internal class Undefined
    {
        [Given("an implemented step")]
        public void GivenAnImplementedStep()
        {
        }

        [Given("a step that will be skipped")]
        public void GivenAStepThatWillBeSkipped()
        {
        }
    }
}
