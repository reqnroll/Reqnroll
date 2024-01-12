using System.Threading.Tasks;
using NUnit.Framework;
using Reqnroll;

namespace Specs
{
    // This class is only required because in this sample application the generator 
    // is not loaded from NuGet package. In a usual Reqnroll project it is not needed.
    [SetUpFixture]
    public class NUnitAssemblyHooks
    {
        [OneTimeSetUp]
        public async Task AssemblyInitialize()
        {
            var currentAssembly = typeof(NUnitAssemblyHooks).Assembly;
            await TestRunnerManager.OnTestRunStartAsync(currentAssembly);
        }

        [OneTimeTearDown]
        public async Task AssemblyCleanup()
        {
            var currentAssembly = typeof(NUnitAssemblyHooks).Assembly;
            await TestRunnerManager.OnTestRunEndAsync(currentAssembly);
        }
    }
}
