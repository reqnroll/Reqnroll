using System.Collections.Generic;

namespace Reqnroll.Plugins
{
    public interface IRuntimePluginLocator
    {
        IReadOnlyList<string> GetAllRuntimePlugins();
    }
}
