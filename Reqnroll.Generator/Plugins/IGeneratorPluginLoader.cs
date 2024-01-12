using Reqnroll.Plugins;

namespace Reqnroll.Generator.Plugins
{
    public interface IGeneratorPluginLoader
    {
        IGeneratorPlugin LoadPlugin(PluginDescriptor pluginDescriptor);
    }
}
