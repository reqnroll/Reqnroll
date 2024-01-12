using System.Reflection;

namespace Reqnroll.Infrastructure
{
    public interface ITestRunnerFactory
    {
        ITestRunner Create(Assembly testAssembly);
    }
}
