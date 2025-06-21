using Reqnroll.UnitTestProvider;
using TUnit.Core.Exceptions;

namespace Reqnroll.TUnit.ReqnrollPlugin;

public class TUnitRuntimeProvider : IUnitTestRuntimeProvider
{
    public void TestPending(string message)
    {
        throw new PendingStepException(message);
    }

    public void TestInconclusive(string message)
    {
        throw new InconclusiveTestException(message, null!);
    }

    public void TestIgnore(string message)
    {
        Skip.Test(message);
    }
}
