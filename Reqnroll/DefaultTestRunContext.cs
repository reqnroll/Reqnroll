using BoDi;
using Reqnroll.TestFramework;

namespace Reqnroll;

public interface ITestRunContext : IReqnrollContext
{
    IObjectContainer TestRunContainer { get; }
    string TestDirectory { get; }
}

public class DefaultTestRunContext : ReqnrollContext, ITestRunContext
{
    private readonly ITestRunSettingsProvider _testRunSettingsProvider;

    public IObjectContainer TestRunContainer { get; }

    public string TestDirectory => _testRunSettingsProvider.GetTestDirectory();

    public DefaultTestRunContext(IObjectContainer testRunContainer, ITestRunSettingsProvider testRunSettingsProvider)
    {
        _testRunSettingsProvider = testRunSettingsProvider;
        TestRunContainer = testRunContainer;
    }
}
