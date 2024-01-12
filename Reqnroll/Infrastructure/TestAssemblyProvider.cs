using System.Reflection;

namespace Reqnroll.Infrastructure
{
    public class TestAssemblyProvider : ITestAssemblyProvider
    {
        public Assembly TestAssembly { get; private set; }

        public void RegisterTestAssembly(Assembly testAssembly)
        {
            TestAssembly = testAssembly;
        }

    }
}
