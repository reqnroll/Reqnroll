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

        [When("a step fails")]
        public void AStepFails()
        {
            throw new Exception("Exception in step");
        }

        // When a step does not exist - no implementation should be generated

        // Hook implementations
        [BeforeScenario]
        public void BeforeScenarioHook() { }

        [AfterScenario]
        public void AfterScenarioHook() { }

    }
}
