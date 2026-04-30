using Reqnroll;
using Reqnroll.UnitTestProvider;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CucumberMessages.CompatibilityTests.CCK.hooks_skipped
{
    [Binding]
    internal class HooksSkipped
    {
        private IUnitTestRuntimeProvider _unitTestRuntimeProvider;

        public HooksSkipped(IUnitTestRuntimeProvider unitTestRuntimeProvider)
        {
            _unitTestRuntimeProvider = unitTestRuntimeProvider;
        }

        [BeforeScenario(Order = 1)]
        public void BeforeScenarioNormalEarly()
        {
        }

        [BeforeScenario("@skip-before", Order=2)]
        public void BeforeScenarioWithSkip()
        {
            _unitTestRuntimeProvider.TestIgnore("Skipped before scenario");
        }

        [BeforeScenario(Order = 3)]
        public void BeforeScenarioNormalLate()
        {
        }

        [AfterScenario(Order = 1)]
        public void AfterScenarioWithSkipEarly()
        {
        }

        [AfterScenario("@skip-after", Order = 2)]
        public void AfterScenarioWithSkip()
        {
            _unitTestRuntimeProvider.TestIgnore("Skipped after scenario");
        }

        [AfterScenario(Order = 3)]
        public void AfterScenarioWithSkipLate()
        {
        }

        [Given("a normal step")]
        public void GivenAStepThatDoesNotSkip()
        {
        }

        [Given("a step that skips")]
        public void GivenISkipAStep()
        {
            _unitTestRuntimeProvider.TestIgnore("skipped");
        }
    }
}
