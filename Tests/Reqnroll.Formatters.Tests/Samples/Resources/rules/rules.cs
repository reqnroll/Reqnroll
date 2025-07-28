using Reqnroll;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CucumberMessages.CompatibilityTests.CCK.rules
{
    [Binding]
    internal class Rules
    {
        private int Money;
        private Stack<string> Stock = new();
        private string _chocolate = "";

        [Given("the customer has {int} cents")]
        public void GivenTheCustomerHasCents(int p0)
        {
            Money = p0;
        }

        [Given("there are chocolate bars in stock")]
        public void GivenThereAreChocolateBarsInStock()
        {
            Stock = new Stack<string>();
            Stock.Push("Mars");
        }

        [Given("there are no chocolate bars in stock")]
        public void GivenThereAreNoChocolateBarsInStock()
        {
            Stock = new Stack<string>();
        }

        [When("the customer tries to buy a {int} cent chocolate bar")]
        public void WhenTheCustomerTriesToBuyACentChocolateBar(int p0)
        {
            if (Money >= p0)
            {
                if (!Stock.TryPop(out _chocolate!))
                    _chocolate = "";
            }
        }

        [Then("the sale should not happen")]
        public void ThenTheSaleShouldNotHappen()
        {
            if (!string.IsNullOrEmpty(_chocolate))
                throw new Exception("Sale should not happen");
        }

        [Then("the sale should happen")]
        public void ThenTheSaleShouldHappen()
        {
            if (string.IsNullOrEmpty(_chocolate))
                throw new Exception("Sale should happen");
        }

    }
}
