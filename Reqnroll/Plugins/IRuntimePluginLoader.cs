using Reqnroll.Tracing;

namespace Reqnroll.Plugins
{
    public interface IRuntimePluginLoader
    {
        IRuntimePlugin LoadPlugin(string pluginAssemblyName, ITraceListener traceListener, bool traceMissingPluginAttribute);
    }
}