using Reqnroll;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CucumberMessages.CompatibilityTests.CCK.hooks
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
        [AfterScenario]
        public void ScenarioHook() { }

        [AfterScenario(new string[] { "some-tag", "some-other-tag" })]
        public void FailingAfterHook()
        {
            throw new Exception("Exception in conditional hook");
        }

        [AfterScenario("with-attachment")]
        public void PassingAfterHook()
        {
            reqnrollOutputHelper.AddAttachment("cucumber.svg");
        }

    }
}
