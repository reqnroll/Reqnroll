using System;
using Reqnroll.BoDi;
using FluentAssertions;
using Xunit;
using Reqnroll.Generator.Configuration;
using Reqnroll.Generator.Plugins;
using Reqnroll.UnitTestProvider;

namespace Reqnroll.GeneratorTests
{
    
    public class GeneratorPluginTests
    {
        [Fact]
        public void Should_be_able_to_register_dependencies_from_a_plugin()
        {
            var pluginWithCustomDependency = new PluginWithCustomDependency();
            var generatorPluginEvents = new GeneratorPluginEvents();
            
            pluginWithCustomDependency.Initialize(generatorPluginEvents, new GeneratorPluginParameters(), new UnitTestProviderConfiguration());


            var objectContainer = new ObjectContainer();
            generatorPluginEvents.RaiseRegisterDependencies(objectContainer);


            var customDependency = objectContainer.Resolve<ICustomDependency>();
            customDependency.Should().BeOfType(typeof(CustomDependency));
        }

        [Fact]
        public void Should_be_able_to_change_default_configuration_from_a_plugin()
        {
            var pluginWithCustomConfiguration = new PluginWithCustomConfiguration(conf => conf.ReqnrollConfiguration.StopAtFirstError = true);
            var generatorPluginEvents = new GeneratorPluginEvents();

            pluginWithCustomConfiguration.Initialize(generatorPluginEvents, new GeneratorPluginParameters(), new UnitTestProviderConfiguration());
            
            var reqnrollProjectConfiguration = new ReqnrollProjectConfiguration();
            generatorPluginEvents.RaiseConfigurationDefaults(reqnrollProjectConfiguration);

            reqnrollProjectConfiguration.ReqnrollConfiguration.StopAtFirstError.Should().BeTrue();
        }

        [Fact]
        public void Should_be_able_to_register_further_dependencies_based_on_the_configuration() //generatorPluginEvents.RaiseCustomizeDependencies();
        {
            var pluginWithCustomization = new PluginWithCustomization();
            var generatorPluginEvents = new GeneratorPluginEvents();

            pluginWithCustomization.Initialize(generatorPluginEvents, new GeneratorPluginParameters(), new UnitTestProviderConfiguration());

            var container = new ObjectContainer();
            var reqnrollProjectConfiguration = new ReqnrollProjectConfiguration();
            generatorPluginEvents.RaiseCustomizeDependencies(container, reqnrollProjectConfiguration);
            container.ResolveAll<ICustomDependency>().Should().BeEmpty();

            reqnrollProjectConfiguration.ReqnrollConfiguration.StopAtFirstError = true;
            generatorPluginEvents.RaiseCustomizeDependencies(container, reqnrollProjectConfiguration);
            
            var customHeaderWriter = container.Resolve<ICustomDependency>();
            customHeaderWriter.Should().BeOfType<CustomDependency>();
        }

        //[Fact]
        //public void Should_be_able_to_specify_a_plugin_with_parameters()
        //{
        //    var configurationHolder = new ReqnrollConfigurationHolder(ConfigSource.AppConfig, string.Format(@"<reqnroll>
        //          <plugins>
        //            <add name=""MyCompany.MyPlugin"" parameters=""foo, bar"" />
        //          </plugins>
        //        </reqnroll>"));
        //    var pluginMock = new Mock<IGeneratorPlugin>();
        //    GeneratorContainerBuilder.DefaultDependencyProvider = new TestDefaultDependencyProvider(pluginMock.Object);
        //    CreateDefaultContainer(configurationHolder);

        //    pluginMock.Verify(p => p.Initialize(It.IsAny<GeneratorPluginEvents>(), It.Is<GeneratorPluginParameters>(pp => pp.Parameters == "foo, bar"), It.IsAny<UnitTestProviderConfiguration>()));
        //}

        //private ReqnrollConfigurationHolder GetConfigWithPlugin()
        //{
        //    return new ReqnrollConfigurationHolder(ConfigSource.AppConfig, string.Format(@"<reqnroll>
        //          <plugins>
        //            <add name=""MyCompany.MyPlugin"" />
        //          </plugins>
        //        </reqnroll>"));
        //}

        //private IObjectContainer CreateDefaultContainer(ReqnrollConfigurationHolder configurationHolder)
        //{
        //    return new GeneratorContainerBuilder().CreateContainer(configurationHolder, new ProjectSettings(), Enumerable.Empty<GeneratorPluginInfo>());
        //}

        //class TestDefaultDependencyProvider : DefaultDependencyProvider
        //{
        //    private readonly IGeneratorPlugin pluginToReturn;

        //    public TestDefaultDependencyProvider(IGeneratorPlugin pluginToReturn)
        //    {
        //        this.pluginToReturn = pluginToReturn;
        //    }

        //    public override void RegisterDefaults(ObjectContainer container)
        //    {
        //        base.RegisterDefaults(container);

        //        var pluginLoaderStub = new Mock<IGeneratorPluginLoader>();
        //        pluginLoaderStub.Setup(pl => pl.LoadPlugin(It.IsAny<PluginDescriptor>())).Returns(pluginToReturn);
        //        container.RegisterInstanceAs<IGeneratorPluginLoader>(pluginLoaderStub.Object);
        //    }
        //}

        public interface ICustomDependency
        {

        }

        public class CustomDependency : ICustomDependency
        {

        }

        public class PluginWithCustomDependency : IGeneratorPlugin
        {
            public void Initialize(GeneratorPluginEvents generatorPluginEvents, GeneratorPluginParameters generatorPluginParameters, UnitTestProviderConfiguration unitTestProviderConfiguration)
            {
                generatorPluginEvents.RegisterDependencies += (_, args) => args.ObjectContainer.RegisterTypeAs<CustomDependency, ICustomDependency>();
            }
        }

        public class PluginWithCustomization : IGeneratorPlugin
        {
            public void Initialize(GeneratorPluginEvents generatorPluginEvents, GeneratorPluginParameters generatorPluginParameters, UnitTestProviderConfiguration unitTestProviderConfiguration)
            {
                generatorPluginEvents.CustomizeDependencies += (_, args) =>
                {
                    if (args.ReqnrollProjectConfiguration.ReqnrollConfiguration.StopAtFirstError)
                        args.ObjectContainer.RegisterTypeAs<CustomDependency, ICustomDependency>();
                };
            }
            
        }

        public class PluginWithCustomConfiguration : IGeneratorPlugin
        {
            private readonly Action<ReqnrollProjectConfiguration> _specifyDefaults;

            public PluginWithCustomConfiguration(Action<ReqnrollProjectConfiguration> specifyDefaults)
            {
                _specifyDefaults = specifyDefaults;
            }

            public void Initialize(GeneratorPluginEvents generatorPluginEvents, GeneratorPluginParameters generatorPluginParameters, UnitTestProviderConfiguration unitTestProviderConfiguration)
            {
                generatorPluginEvents.ConfigurationDefaults += (_, args) => { _specifyDefaults(args.ReqnrollProjectConfiguration); };
            }
        }
    }
}
