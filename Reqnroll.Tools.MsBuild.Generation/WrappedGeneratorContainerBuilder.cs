using System.Collections.Generic;
using BoDi;
using Reqnroll.Analytics;
using Reqnroll.Configuration;
using Reqnroll.Generator;
using Reqnroll.Generator.Interfaces;
using Reqnroll.Tracing;

namespace Reqnroll.Tools.MsBuild.Generation
{
    public class WrappedGeneratorContainerBuilder
    {
        private readonly GeneratorContainerBuilder _generatorContainerBuilder;
        private readonly GenerateFeatureFileCodeBehindTaskConfiguration _generateFeatureFileCodeBehindTaskConfiguration;

        public WrappedGeneratorContainerBuilder(GeneratorContainerBuilder generatorContainerBuilder, GenerateFeatureFileCodeBehindTaskConfiguration generateFeatureFileCodeBehindTaskConfiguration)
        {
            _generatorContainerBuilder = generatorContainerBuilder;
            _generateFeatureFileCodeBehindTaskConfiguration = generateFeatureFileCodeBehindTaskConfiguration;
        }

        public IObjectContainer BuildGeneratorContainer(
            ReqnrollConfigurationHolder reqnrollConfigurationHolder,
            ProjectSettings projectSettings,
            IReadOnlyCollection<GeneratorPluginInfo> generatorPluginInfos,
            IObjectContainer rootObjectContainer)
        {
            var objectContainer = _generatorContainerBuilder.CreateContainer(reqnrollConfigurationHolder, projectSettings, generatorPluginInfos, rootObjectContainer);

            objectContainer.RegisterTypeAs<ProjectCodeBehindGenerator, IProjectCodeBehindGenerator>();
            objectContainer.RegisterTypeAs<AnalyticsEventProvider, IAnalyticsEventProvider>();
            objectContainer.RegisterTypeAs<MSBuildTraceListener, ITraceListener>();

            if (_generateFeatureFileCodeBehindTaskConfiguration.OverrideFeatureFileCodeBehindGenerator is null)
            {
                objectContainer.RegisterTypeAs<FeatureFileCodeBehindGenerator, IFeatureFileCodeBehindGenerator>();
            }
            else
            {
                objectContainer.RegisterInstanceAs(_generateFeatureFileCodeBehindTaskConfiguration.OverrideFeatureFileCodeBehindGenerator);
            }

            objectContainer.Resolve<IConfigurationLoader>().TraceConfigSource(objectContainer.Resolve<ITraceListener>(), objectContainer.Resolve<ReqnrollConfiguration>());

            return objectContainer;
        }
    }
}
