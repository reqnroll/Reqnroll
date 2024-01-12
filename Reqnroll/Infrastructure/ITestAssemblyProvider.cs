using System.Reflection;

namespace Reqnroll.Infrastructure
{
    public interface ITestAssemblyProvider
    {
        Assembly TestAssembly { get; }

        public void RegisterTestAssembly(Assembly testAssembly);
    }
}
