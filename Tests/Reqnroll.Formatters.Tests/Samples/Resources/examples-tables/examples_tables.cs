using Reqnroll;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CucumberMessages.CompatibilityTests.CCK.examples_tables
{
    [Binding]
    internal class Example_tables
    {
        private int _count;
        private int _friends;

        [Given("there are {int} cucumbers")]
        public void GivenThereAreCucumbers(int p0)
        {
            _count = p0;
        }

        [Given("there are {int} friends")]
        public void GivenThereAreFriends(int p0)
        {
            _friends = p0;
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

        [Then("each person can eat {int} cucumbers")]
        public void ThenEachPersonCanEatCucumbers(int p0)
        {
            var share = Math.Floor((double)_count / (1 + _friends));

            Assert.AreEqual(p0, share);
        }
    }
}
