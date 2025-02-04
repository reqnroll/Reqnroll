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
        [Given(@"a step that matches more than one step binding")]
        public void FirstMatchingStep() { }

        [Given(@"a step that matches more than one step binding")]
        public void SecondMatchingStep() { }

        [Then(@"this step gets skipped because of the prior ambiguous step")]
        public void ThirdSkippedStep() { }
    }
}
