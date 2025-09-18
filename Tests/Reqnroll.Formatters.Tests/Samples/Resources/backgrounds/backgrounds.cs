using Reqnroll;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CucumberMessages.CompatibilityTests.CCK.background
{
    [Binding]
    internal class Background
    {
        [Given("an order for {string}")]
        public void GivenAnOrder(string item) { }

        [When("an action")]
        public void WhenAnAction() { }

        [Then("an outcome")]
        public void ThenAnOutcome() { }
    }
}
