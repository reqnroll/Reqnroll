using FluentAssertions;
using Reqnroll.BoDi;
using Reqnroll.Generator.Configuration;
using Reqnroll.Generator.Plugins;
using Reqnroll.Generator.UnitTestConverter;
using Reqnroll.UnitTestProvider;
using System;
using System.Collections.Generic;
using Xunit;

namespace Reqnroll.GeneratorTests;

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
        container.ResolveAll<ITagFilterMatcher>().Should().BeEmpty();

        reqnrollProjectConfiguration.ReqnrollConfiguration.StopAtFirstError = true;
        generatorPluginEvents.RaiseCustomizeDependencies(container, reqnrollProjectConfiguration);
            
        var customizedDependency = container.Resolve<ITagFilterMatcher>();
        customizedDependency.Should().BeOfType<CustomTagFilterMatcher>();
    }

    public interface ICustomDependency;

    public class CustomDependency : ICustomDependency;

    public class CustomTagFilterMatcher : ITagFilterMatcher
    {
        public bool Match(string tagFilter, IEnumerable<string> tagNames) => throw new NotImplementedException();

        public bool MatchPrefix(string tagFilter, IEnumerable<string> tagNames) => throw new NotImplementedException();

        public bool GetTagValue(string tagFilter, IEnumerable<string> tagNames, out string value) => throw new NotImplementedException();

        public string[] GetTagValues(string tagFilter, IEnumerable<string> tagNames) => throw new NotImplementedException();
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
                    args.ObjectContainer.RegisterTypeAs<CustomTagFilterMatcher, ITagFilterMatcher>();
            };
        }
            
    }

    public class PluginWithCustomConfiguration(Action<ReqnrollProjectConfiguration> specifyDefaults) : IGeneratorPlugin
    {
        public void Initialize(GeneratorPluginEvents generatorPluginEvents, GeneratorPluginParameters generatorPluginParameters, UnitTestProviderConfiguration unitTestProviderConfiguration)
        {
            generatorPluginEvents.ConfigurationDefaults += (_, args) => { specifyDefaults(args.ReqnrollProjectConfiguration); };
        }
    }
}