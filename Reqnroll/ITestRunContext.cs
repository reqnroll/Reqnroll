using Reqnroll.BoDi;

namespace Reqnroll;

public interface ITestRunContext : IReqnrollContext
{
    IObjectContainer TestRunContainer { get; }
    string TestDirectory { get; }
}
