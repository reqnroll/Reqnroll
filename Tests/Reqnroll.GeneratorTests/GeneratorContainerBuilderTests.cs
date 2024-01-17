using System.Linq;
using FluentAssertions;
using Xunit;
using Reqnroll.Configuration;
using Reqnroll.Generator;
using Reqnroll.Generator.Interfaces;

namespace Reqnroll.GeneratorTests
{
    public class GeneratorContainerBuilderTests
    {
        [Fact]
        public void Should_create_a_container()
        {
            var container = new GeneratorContainerBuilder().CreateContainer(new ReqnrollConfigurationHolder(ConfigSource.Default, null), new ProjectSettings(), Enumerable.Empty<GeneratorPluginInfo>());
            container.Should().NotBeNull();
        }

        [Fact]
        public void Should_register_generator_configuration_with_default_config()
        {
            var container = new GeneratorContainerBuilder().CreateContainer(new ReqnrollConfigurationHolder(ConfigSource.Default, null), new ProjectSettings(), Enumerable.Empty<GeneratorPluginInfo>());
            container.Resolve<Configuration.ReqnrollConfiguration>().Should().NotBeNull();
        }

        [Fact]
        public void Should_register_generator_with_custom_settings_when_configured()
        {
            var container = new GeneratorContainerBuilder().CreateContainer(
                new ReqnrollConfigurationHolder(
                    ConfigSource.Json,
                    """
                    {
                        "generator": {
                          "allowDebugGeneratedFiles": true
                        }
                    }
                    """), new ProjectSettings(), Enumerable.Empty<GeneratorPluginInfo>());
            container.Resolve<ReqnrollConfiguration>().AllowDebugGeneratedFiles.Should().Be(true);
        }
    }
}
