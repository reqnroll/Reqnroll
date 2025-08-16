using Reqnroll.BoDi;

namespace Reqnroll;

public interface ITestThreadContext : IReqnrollContext
{
    IObjectContainer TestThreadContainer { get; }
}
