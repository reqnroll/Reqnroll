using Reqnroll.UnitTestProvider;

namespace Reqnroll.xUnit3.ReqnrollPlugin;

public class XUnit3RuntimeProvider : IUnitTestRuntimeProvider
{
    public void TestPending(string message) => Xunit.Assert.Skip(message);

    public void TestInconclusive(string message) => throw new XUnitInconclusiveException($"Test inconclusive: {message}");

    public void TestIgnore(string message) => Xunit.Assert.Skip(message);
}
