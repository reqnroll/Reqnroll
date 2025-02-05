using Reqnroll.BoDi;
using Reqnroll.Plugins;

namespace Reqnroll.PlatformCompatibility;
public static class PlatformHelper
{
    public static void RegisterPluginAssemblyLoader(IObjectContainer container)
    {
        if (PlatformInformation.IsDotNetFramework)
            container.RegisterTypeAs<DotNetFrameworkPluginAssemblyLoader, IPluginAssemblyLoader>();
        else
            container.RegisterTypeAs<DotNetCorePluginAssemblyLoader, IPluginAssemblyLoader>();
    }
}
