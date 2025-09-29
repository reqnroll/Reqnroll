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
    internal class Hooks
    {
        private readonly IReqnrollOutputHelper reqnrollOutputHelper;

        public Hooks(IReqnrollOutputHelper reqnrollOutputHelper)
        {
            this.reqnrollOutputHelper = reqnrollOutputHelper;
        }

        [When("a step passes")]
        public void AStepPasses()
        {
        }

        // Hook implementations
        [BeforeScenario("@passing-hook")]
        public void BeforeScenarioHook() { }

        [BeforeScenario("@fail-before")]
        public void BeforeScenarioHookThatFails() {
            throw new Exception("Exception in conditional hook");
        }

        [AfterScenario("@fail-after")]
        public void FailingAfterHook()
        {
            throw new Exception("Exception in conditional hook");
        }

        [AfterScenario("@passing-hook")]
        public void AfterScenarioHook() { }
    }
}
