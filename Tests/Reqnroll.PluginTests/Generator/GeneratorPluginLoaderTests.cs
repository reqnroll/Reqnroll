using FluentAssertions;
using Reqnroll.Generator.Plugins;
using Reqnroll.Plugins;
using Reqnroll.xUnit.Generator.ReqnrollPlugin;
using Xunit;

namespace Reqnroll.PluginTests.Generator
{
    public class GeneratorPluginLoaderTests
    {
        [Fact]
        public void LoadPlugin_LoadXUnitSuccessfully()
        {
            //ARRANGE
            var generatorPluginLoader = new GeneratorPluginLoader(new DotNetCorePluginAssemblyLoader());

            //ACT
            var pluginDescriptor = new PluginDescriptor("Reqnroll.xUnit.Generator.ReqnrollPlugin", "Reqnroll.xUnit.Generator.ReqnrollPlugin.dll", PluginType.Generator, "");
            var generatorPlugin = generatorPluginLoader.LoadPlugin(pluginDescriptor);

            //ASSERT
            generatorPlugin.Should().BeOfType<GeneratorPlugin>();
        }

    }
}