using Reqnroll.EnvironmentAccess;

namespace Reqnroll.TestFramework
{
    public class DefaultTestRunSettingsProvider : ITestRunSettingsProvider
    {
        private readonly IEnvironmentWrapper _environmentWrapper;

        public DefaultTestRunSettingsProvider(IEnvironmentWrapper environmentWrapper)
        {
            _environmentWrapper = environmentWrapper;
        }

        public string GetTestDirectory() => _environmentWrapper.GetCurrentDirectory();
    }
}
