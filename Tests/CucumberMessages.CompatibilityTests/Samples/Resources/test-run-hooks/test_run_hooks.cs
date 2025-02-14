using Reqnroll;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;

namespace CucumberMessages.CompatibilityTests.CCK.TestRunHooks
{
    [Binding]
    internal class TestRunHooks
    {
        private readonly IReqnrollOutputHelper reqnrollOutputHelper;

        public TestRunHooks(IReqnrollOutputHelper reqnrollOutputHelper)
        {
            this.reqnrollOutputHelper = reqnrollOutputHelper;
        }

        [When("a step passes")]
        public void AStepPasses()
        {
        }

        [BeforeTestRun]
        public static void BeforeTestHookMethod()
        {
        }

        [AfterTestRun]
        public static void AfterTestHookMethod()
        {
        }

        [BeforeFeature]
        public static void BeforeFeatureHookMethod(IReqnrollOutputHelper reqnrollOutputHelper)
        {
            reqnrollOutputHelper.WriteLine("Before Feature Hook Executed.");
        }

        [AfterFeature]
        public static void AfterFeatureHookMethod(IReqnrollOutputHelper reqnrollOutputHelper)
        {
            reqnrollOutputHelper.WriteLine("After Feature Hook Executed.");
        }
    }
}
