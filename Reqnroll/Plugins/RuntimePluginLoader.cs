using System;
using System.Reflection;
using Reqnroll.PlatformCompatibility;
using Reqnroll.Tracing;

namespace Reqnroll.Plugins
{
    public class RuntimePluginLoader(IPluginAssemblyLoader _pluginAssemblyLoader) : IRuntimePluginLoader
    {
        public IRuntimePlugin LoadPlugin(string pluginAssemblyPath, ITraceListener traceListener, bool traceMissingPluginAttribute)
        {
            Assembly assembly;
            try
            {
                assembly = _pluginAssemblyLoader.LoadAssembly(pluginAssemblyPath);
            }
            catch (Exception ex)
            {
                throw new ReqnrollException($"Unable to load plugin: {pluginAssemblyPath}. Please check https://go.reqnroll.net/doc-plugins for details. (Framework: {PlatformInformation.DotNetFrameworkDescription})", ex);
            }

            var pluginAttribute = (RuntimePluginAttribute)Attribute.GetCustomAttribute(assembly, typeof(RuntimePluginAttribute));
            if (pluginAttribute == null)
            {
                if (traceMissingPluginAttribute)
                    traceListener.WriteToolOutput($"Missing [assembly:RuntimePlugin] attribute in {assembly.FullName}. Please check https://go.reqnroll.net/doc-plugins for details.");

                return null;
            }

            if (!typeof(IRuntimePlugin).IsAssignableFrom(pluginAttribute.PluginType))
                throw new ReqnrollException($"Invalid plugin attribute in {assembly.FullName}. Plugin type must implement IRuntimePlugin. Please check https://go.reqnroll.net/doc-plugins for details.");

            IRuntimePlugin plugin;
            try
            {
                plugin = (IRuntimePlugin)Activator.CreateInstance(pluginAttribute.PluginType);
            }
            catch (Exception ex)
            {
                throw new ReqnrollException($"Invalid plugin in {assembly.FullName}. Plugin must have a default constructor that does not throw exception. Please check https://go.reqnroll.net/doc-plugins for details.", ex);
            }

            return plugin;
        }
    }
}