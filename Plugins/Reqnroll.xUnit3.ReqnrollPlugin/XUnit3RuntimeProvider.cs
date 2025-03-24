using Reqnroll.UnitTestProvider;

namespace Reqnroll.xUnit3.ReqnrollPlugin;

public class XUnit3RuntimeProvider : IUnitTestRuntimeProvider
{
    public void TestPending(string message) => throw new XUnitPendingStepException(message);

    public void TestInconclusive(string message) => throw new XUnitInconclusiveException(message);

    public void TestIgnore(string message) => throw new XUnitIgnoreException(message);
}
