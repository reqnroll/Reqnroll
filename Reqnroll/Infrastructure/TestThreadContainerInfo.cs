using Reqnroll.BoDi;

namespace Reqnroll.Infrastructure;

internal class TestThreadContainerInfo
{
    internal string Id { get; }

    internal TestThreadContainerInfo(string id)
    {
        Id = id;
    }

    internal static string GetId(IObjectContainer objectContainer)
    {
        if (!objectContainer.IsRegistered<TestThreadContainerInfo>())
            return null;
        var testThreadContainerInfo = objectContainer.Resolve<TestThreadContainerInfo>();
        return testThreadContainerInfo.Id;
    }
}
