using System;
using System.IO;
using System.Reflection;
using Reqnroll.Infrastructure;
using Reqnroll.Plugins;

namespace Reqnroll.Generator.Plugins
{
    public class GeneratorPluginLoader : IGeneratorPluginLoader
    {
        public IGeneratorPlugin LoadPlugin(PluginDescriptor pluginDescriptor)
        {
            Assembly pluginAssembly;
            try
            {

#if NETCOREAPP
                pluginAssembly = PluginAssemblyResolver.Load(pluginDescriptor.Path);
#else
                pluginAssembly = Assembly.LoadFrom(pluginDescriptor.Path);
#endif
            }
            catch(Exception ex)
            {
                throw new ReqnrollException($"Unable to load plugin assembly: {pluginDescriptor.Path}. Please check https://go.reqnroll.net/doc-plugins for details.", ex);
            }

            var pluginAttribute = (GeneratorPluginAttribute)Attribute.GetCustomAttribute(pluginAssembly, typeof(GeneratorPluginAttribute));
            if (pluginAttribute == null)
                throw new ReqnrollException("Missing [assembly:GeneratorPlugin] attribute in " + pluginDescriptor.Path);

            if (!typeof(IGeneratorPlugin).IsAssignableFrom((pluginAttribute.PluginType)))
                throw new ReqnrollException($"Invalid plugin attribute in {pluginDescriptor.Path}. Plugin type must implement IGeneratorPlugin. Please check https://go.reqnroll.net/doc-plugins for details.");

            IGeneratorPlugin plugin;
            try
            {
                plugin = (IGeneratorPlugin)Activator.CreateInstance(pluginAttribute.PluginType);
            }
            catch (Exception ex)
            {
                throw new ReqnrollException($"Invalid plugin in {pluginDescriptor.Path}. Plugin must have a default constructor that does not throw exception. Please check https://go.reqnroll.net/doc-plugins for details.", ex);
            }

            return plugin;
        }
    }
}