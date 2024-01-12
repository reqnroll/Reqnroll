using Microsoft.VisualStudio.TestTools.UnitTesting;
using Reqnroll.UnitTestProvider;

namespace Reqnroll.MSTest.ReqnrollPlugin
{
    public class MsTestRuntimeProvider : IUnitTestRuntimeProvider
    {
        public void TestPending(string message)
        {
            TestInconclusive(message);
        }

        public void TestInconclusive(string message)
        {
            Assert.Inconclusive(message);
        }

        public void TestIgnore(string message)
        {
            TestInconclusive(message); // there is no dynamic "Ignore" in mstest
        }

        public bool DelayedFixtureTearDown => true;
    }
}