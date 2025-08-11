using Reqnroll;
using Reqnroll.UnitTestProvider;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CucumberMessages.CompatibilityTests.CCK.skipped
{
    [Binding]
    internal class Skipped
    {
        private IUnitTestRuntimeProvider _unitTestRuntimeProvider;

        public Skipped(IUnitTestRuntimeProvider unitTestRuntimeProvider)
        {
            _unitTestRuntimeProvider = unitTestRuntimeProvider;
        }
         
        [BeforeScenario("@skip")]
        public void BeforeScenarioWithSkip()
        {
            _unitTestRuntimeProvider.TestIgnore("Skipped before scenario");
        }

        [Given("a step that does not skip")]
        public void GivenAStepThatDoesNotSkip()
        {
        }

        [Given("a step that is skipped")]
        public void GivenAStepThatSkips()
        {
        }

        [Given("I skip a step")]
        public void GivenISkipAStep()
        {
            _unitTestRuntimeProvider.TestIgnore("Skipped");
        }
    }
}
