using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Reqnroll;

namespace Reqnroll.Formatters.Tests.Samples.Resources.rules_backgrounds;

[Binding]
internal class rules_backgrounds
{
    [Given("an order for {string}")]
    public void GivenAnOrder(string order) { }

    [When("an action")]
    public void WhenAnAction() { }

    [Then("an outcome")]
    public void ThenAnOutcome() { }
}
