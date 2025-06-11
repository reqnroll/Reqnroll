using Reqnroll;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CucumberMessages.CompatibilityTests.CCK.stack_traces
{
    [Binding]
    internal class stack_traces
    {
        [When(@"a step throws an exception")]
        public void WhenAStepThrowsAnException()
        {
            throw new Exception("BOOM");
        }
    }
}
