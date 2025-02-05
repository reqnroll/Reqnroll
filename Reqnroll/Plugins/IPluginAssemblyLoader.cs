using System.Reflection;

namespace Reqnroll.Plugins;
public interface IPluginAssemblyLoader
{
    Assembly LoadAssembly(string assemblyPath);
}