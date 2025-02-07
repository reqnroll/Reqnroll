// Used by Reqnroll.SystemTests.Plugins.XRetryPluginTest
using System.Collections.Generic;
namespace XRetryPluginTest.StepDefinitions
{
    [Binding]
    public class XRetryPluginTestStepDefinitions
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
                throw new Exception($"simulated error for {label}");
            }
        }
    }
}
