using Reqnroll;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CucumberMessages.CompatibilityTests.CCK.Minimal
{
    [Binding]
    public class Minimal
    {
        [Given("I have {int} cukes in my belly")]
        public void GivenIHaveCukesInMyBelly(int p0)
        {
            // pass
        }
    }
}
