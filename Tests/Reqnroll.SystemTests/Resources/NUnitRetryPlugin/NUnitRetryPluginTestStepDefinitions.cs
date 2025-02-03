// Used by Reqnroll.SystemTests.Plugins.NUnitRetryPluginTest
using System.Collections.Generic;
using NUnit.Framework;

namespace NUnitRetryPluginTest.StepDefinitions
{
    [Binding]
    public class NUnitRetryPluginTestStepDefinitions
    {
        private static readonly Dictionary<string, int> RetriesByLabel = new Dictionary<string, int>();

        [When("fail for first {int} times {word}")]
        public void WhenFailForFirstTwoTimes(int retryCount, string label)
        {
            if (!RetriesByLabel.TryGetValue(label, out var retries))
            {
                retries = 0;
            }
            var failTest = retries < retryCount;
            RetriesByLabel[label] = ++retries;
            if (failTest)
            {
                Log.LogCustom("simulated-error", label);
            }
            Assert.That(failTest, Is.False);
        }
    }
}
