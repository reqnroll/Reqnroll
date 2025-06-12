using System.Reflection;
using System.Threading.Tasks;

namespace Reqnroll
{
    // This class does not implement IAsyncDisposable, because that is only supported properly in .NET Standard 2.1, so won't work with .NET 4.6.1. The 'Microsoft.Bcl.AsyncInterfaces' package that overcomes this causes other issues, see https://github.com/simpleinjector/SimpleInjector/issues/867
    public interface ITestRunnerManager
    {
        Assembly TestAssembly { get; }
        Assembly[] BindingAssemblies { get; }
        bool IsMultiThreaded { get; }
        ITestRunner GetTestRunner(FeatureInfo featureHint = null);
        void ReleaseTestRunnerToPool(ITestRunner testRunner);
        Task EndFeatureForAvailableTestRunnersAsync(FeatureInfo featureInfo);
        void Initialize(Assembly testAssembly);
        Task FireTestRunEndAsync();
        Task FireTestRunStartAsync();
        Task DisposeAsync();
    }
}
