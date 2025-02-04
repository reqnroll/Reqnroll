using Reqnroll;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CucumberMessages.CompatibilityTests.CCK.cdata
{
    [Binding]
    internal class Cdata
    {
        [Given("I have {int} <![CDATA[cukes]]> in my belly")]
        public void GivenIHaveCukesInMyBelly(int p0)
        {
        }
    }
}
