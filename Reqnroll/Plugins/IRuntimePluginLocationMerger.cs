using System.Collections.Generic;

namespace Reqnroll.Plugins
{
    public interface IRuntimePluginLocationMerger
    {
        IReadOnlyList<string> Merge(IReadOnlyList<string> pluginPaths);
    }
}