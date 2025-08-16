using Reqnroll.BoDi;
using Reqnroll.TestFramework;

namespace Reqnroll;

public class DefaultTestRunContext(IObjectContainer testRunContainer, ITestRunSettingsProvider testRunSettingsProvider) 
    : ReqnrollContext, ITestRunContext
{
    public IObjectContainer TestRunContainer { get; } = testRunContainer;

    public string TestDirectory => testRunSettingsProvider.GetTestDirectory();
}
