using NUnit.Framework;
using Reqnroll.UnitTestProvider;

namespace Reqnroll.NUnit.ReqnrollPlugin
{
    public class NUnitRuntimeProvider : IUnitTestRuntimeProvider
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
            Assert.Ignore(message);
        }
    }
}