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
        private Dictionary<string, int> accounts = new Dictionary<string, int>();
        private int total = 0;

        [Given(@"I have ${int} in my {word} account")]
        public void AddMoneyToAccount(int amount, string account)
        {
            accounts[account] = amount;
        }

        [When("the accounts are combined")]
        public void CombineAccounts()
        {
            total = accounts.Sum(x => x.Value);
        }

        [Then(@"I have ${int}")]
        public void CheckTotalBalance(int amount)
        {
            if(total != amount) throw new ApplicationException("Total balance should be " + amount);
        }

        [When(@"I transfer ${int} from {word} to {word}")]
        public void TransferMoney(int amount, string from, string to)
        {
            accounts[from] -= amount;
            accounts[to] += amount;
        }

        [Then(@"My {word} account has a balance of ${int}")]
        public void CheckAccountBalance(string account, int balance)
        {
            if(accounts[account] != balance) throw new ApplicationException($"Account: {account} balance should be " + balance);
        }
    }
}
