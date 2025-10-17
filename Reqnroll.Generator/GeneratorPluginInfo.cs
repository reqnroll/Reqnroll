using System.Collections.Generic;
using System.Linq;
using Reqnroll.Generator.Plugins;

namespace Reqnroll.Generator
{
    public class GeneratorPluginInfo
    {
        public GeneratorPluginInfo(string pathToGeneratorPluginAssembly, IDictionary<string, string> pluginParameters)
        {
            PathToGeneratorPluginAssembly = pathToGeneratorPluginAssembly;
            PluginParameters = pluginParameters;
        }

        public string PathToGeneratorPluginAssembly { get; }
        public IDictionary<string, string> PluginParameters { get; }

        /// <summary>
        /// This can be removed with Reqnroll v4 if we change the <see cref="GeneratorPluginParameters.Parameters"/> to a dictionary.
        /// </summary>
        public string GetLegacyPluginParameters()
        {
            return string.Join(";", PluginParameters.Select(kv => $"{kv.Key}={kv.Value}"));
        }
    }
}
