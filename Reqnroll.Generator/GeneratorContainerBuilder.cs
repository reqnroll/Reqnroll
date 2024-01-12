using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using BoDi;
using Reqnroll.Configuration;
using Reqnroll.Generator.CodeDom;
using Reqnroll.Generator.Configuration;
using Reqnroll.Generator.Interfaces;
using Reqnroll.Generator.Plugins;
using Reqnroll.Generator.UnitTestProvider;
using Reqnroll.Plugins;
using Reqnroll.Tracing;
using Reqnroll.UnitTestProvider;

namespace Reqnroll.Generator
{
    public class GeneratorContainerBuilder
    {
        internal static DefaultDependencyProvider DefaultDependencyProvider = new DefaultDependencyProvider();

        public IObjectContainer CreateContainer(ReqnrollConfigurationHolder configurationHolder, ProjectSettings projectSettings, IEnumerable<GeneratorPluginInfo> generatorPluginInfos, IObjectContainer parentObjectContainer = null)
        {
            var container = new ObjectContainer(parentObjectContainer);
            container.RegisterInstanceAs(projectSettings);

            RegisterDefaults(container);

            var configurationProvider = container.Resolve<IGeneratorConfigurationProvider>();
            var generatorPluginEvents = container.Resolve<GeneratorPluginEvents>();
            var unitTestProviderConfiguration = container.Resolve<UnitTestProviderConfiguration>();

            var reqnrollConfiguration = new ReqnrollProjectConfiguration();
            reqnrollConfiguration.ReqnrollConfiguration = configurationProvider.LoadConfiguration(reqnrollConfiguration.ReqnrollConfiguration, configurationHolder);

            LoadPlugins(container, generatorPluginEvents, unitTestProviderConfiguration, generatorPluginInfos.Select(p => p.PathToGeneratorPluginAssembly));
            
            generatorPluginEvents.RaiseRegisterDependencies(container);
            generatorPluginEvents.RaiseConfigurationDefaults(reqnrollConfiguration);

            if (reqnrollConfiguration.ReqnrollConfiguration.GeneratorCustomDependencies != null)
            {
                container.RegisterFromConfiguration(reqnrollConfiguration.ReqnrollConfiguration.GeneratorCustomDependencies);
            }

            container.RegisterInstanceAs(reqnrollConfiguration);
            container.RegisterInstanceAs(reqnrollConfiguration.ReqnrollConfiguration);

            var generatorInfo = container.Resolve<IGeneratorInfoProvider>().GetGeneratorInfo();
            container.RegisterInstanceAs(generatorInfo);

            container.RegisterInstanceAs(container.Resolve<CodeDomHelper>(projectSettings.ProjectPlatformSettings.Language));

            if (unitTestProviderConfiguration != null)
            {
                container.RegisterInstanceAs(container.Resolve<IUnitTestGeneratorProvider>(unitTestProviderConfiguration.UnitTestProvider ?? ConfigDefaults.UnitTestProviderName));
            }

            generatorPluginEvents.RaiseCustomizeDependencies(container, reqnrollConfiguration);

            //container.Resolve<IConfigurationLoader>().TraceConfigSource(container.Resolve<ITraceListener>(), reqnrollConfiguration.ReqnrollConfiguration);

            return container;
        }

        private void LoadPlugins(
            ObjectContainer container,
            GeneratorPluginEvents generatorPluginEvents,
            UnitTestProviderConfiguration unitTestProviderConfiguration,
            IEnumerable<string> generatorPlugins)
        {
            // initialize plugins that were registered from code
            foreach (var generatorPlugin in container.Resolve<IDictionary<string, IGeneratorPlugin>>().Values)
            {
                // these plugins cannot have parameters
                generatorPlugin.Initialize(generatorPluginEvents, new GeneratorPluginParameters(), unitTestProviderConfiguration);
            }

            var pluginLoader = container.Resolve<IGeneratorPluginLoader>();

            foreach (string generatorPlugin in generatorPlugins)
            {
                //todo: should set the parameters, and do not pass empty
                var pluginDescriptor = new PluginDescriptor(Path.GetFileNameWithoutExtension(generatorPlugin), generatorPlugin, PluginType.Generator, string.Empty); 
                LoadPlugin(pluginDescriptor, pluginLoader, generatorPluginEvents, unitTestProviderConfiguration);
            }
        }

        private void LoadPlugin(
            PluginDescriptor pluginDescriptor,
            IGeneratorPluginLoader pluginLoader,
            GeneratorPluginEvents generatorPluginEvents,
            UnitTestProviderConfiguration unitTestProviderConfiguration)
        {
            var plugin = pluginLoader.LoadPlugin(pluginDescriptor);
            var generatorPluginParameters = new GeneratorPluginParameters
            {
                Parameters = pluginDescriptor.Parameters
            };

            plugin.Initialize(generatorPluginEvents, generatorPluginParameters, unitTestProviderConfiguration);
        }

        private void RegisterDefaults(ObjectContainer container)
        {
            DefaultDependencyProvider.RegisterDefaults(container);
        }
    }
}