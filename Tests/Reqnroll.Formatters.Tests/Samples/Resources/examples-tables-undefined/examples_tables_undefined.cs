using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Reqnroll;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Reqnroll.Formatters.Tests.Samples.Resources.examples_tables_undefined;

[Binding]
internal class examples_tables_undefined
{
    private int _count;

    [Given("there are {int} cucumbers")]
    public void GivenThereAreCucumbers(int p0)
    {
        _count = p0;
    }

    [When("I eat {int} cucumbers")]
    public void WhenIEatCucumbers(int p0)
    {
        _count -= p0;
    }

    [Then("I should have {int} cucumbers")]
    public void ThenIShouldHaveCucumbers(int expectedCount)
    {
        Assert.AreEqual(expectedCount, _count);
    }

}
