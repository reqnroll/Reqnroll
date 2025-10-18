using System;
using Reqnroll.Generator.Plugins;
using Reqnroll.Infrastructure;
using Reqnroll.UnitTestProvider;

[assembly: GeneratorPlugin(typeof(Reqnroll.MSTest.Generator.ReqnrollPlugin.GeneratorPlugin))]

namespace Reqnroll.MSTest.Generator.ReqnrollPlugin
{
    public class GeneratorPlugin : IGeneratorPlugin
    {
        public void Initialize(GeneratorPluginEvents generatorPluginEvents, GeneratorPluginParameters generatorPluginParameters, UnitTestProviderConfiguration unitTestProviderConfiguration)
        {
            var parameters = generatorPluginParameters.GetParametersAsDictionary();
            if (parameters.TryGetValue("TargetMsTestVersion", out string version) && IsMsTestV4OrHigher(version))
            {
                unitTestProviderConfiguration.UseUnitTestProvider("mstest4");
            }
            else
            {
                unitTestProviderConfiguration.UseUnitTestProvider("mstest");
            }
        }

        private bool IsMsTestV4OrHigher(string version)
        {
            return !string.IsNullOrEmpty(version) && 
                   int.TryParse(version.Split(['.'], 2)[0], out var majorVersion) && 
                   majorVersion >= 4;
        }
    }
}
