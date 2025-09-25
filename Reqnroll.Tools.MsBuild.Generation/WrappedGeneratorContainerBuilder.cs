using System.Collections.Generic;
using Reqnroll.BoDi;
using Reqnroll.Analytics;
using Reqnroll.Configuration;
using Reqnroll.Generator;
using Reqnroll.Generator.Interfaces;
using Reqnroll.Tracing;

namespace Reqnroll.Tools.MsBuild.Generation
{
    public class WrappedGeneratorContainerBuilder(
        GeneratorContainerBuilder generatorContainerBuilder, 
        IGenerateFeatureFileCodeBehindTaskDependencyCustomizations dependencyCustomizations)
    {
        public IObjectContainer BuildGeneratorContainer(
            ReqnrollConfigurationHolder reqnrollConfigurationHolder,
            ProjectSettings projectSettings,
            IReadOnlyCollection<GeneratorPluginInfo> generatorPluginInfos,
            IObjectContainer rootObjectContainer)
        {
            var objectContainer = generatorContainerBuilder.CreateContainer(reqnrollConfigurationHolder, projectSettings, generatorPluginInfos, rootObjectContainer);

            objectContainer.RegisterTypeAs<AnalyticsEventProvider, IAnalyticsEventProvider>();
            objectContainer.RegisterTypeAs<MSBuildTraceListener, ITraceListener>();
            objectContainer.RegisterTypeAs<FeatureFileCodeBehindGenerator, IFeatureFileCodeBehindGenerator>();

            dependencyCustomizations.CustomizeGeneratorContainerDependencies(objectContainer);

            objectContainer.Resolve<IConfigurationLoader>().TraceConfigSource(objectContainer.Resolve<ITraceListener>(), objectContainer.Resolve<ReqnrollConfiguration>());

            return objectContainer;
        }
    }
}
