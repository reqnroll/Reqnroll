using Reqnroll;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;

namespace CucumberMessages.CompatibilityTests.CCK.hooksMulti
{
    [Binding]
    internal class HooksUndefined
    {
        public HooksUndefined()
        {
        }

        // When a step does not exist - no implementation should be generated

        // Hook implementations
        [BeforeScenario]
        public void BeforeScenarioHook() { }

        [AfterScenario]
        public void AfterScenarioHook() { }

    }
}
