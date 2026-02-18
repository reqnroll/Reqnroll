using Reqnroll;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CucumberMessages.CompatibilityTests.CCK.ambiguous
{
    [Binding]
    internal class Ambiguous
    {
        [Given(@"^a (.*?) with (.*?)$")]
        public void FirstMatchingStep(string p0, string p1) { }

        [Given(@"^a step with (.*)$")]
        public void SecondMatchingStep(string p0) { }
    }
}
