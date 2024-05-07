using Reqnroll.ExternalData.ReqnrollPlugin;
using Reqnroll.ExternalData.ReqnrollPlugin.DataSources;
using Reqnroll.ExternalData.ReqnrollPlugin.Loaders;
using Reqnroll.Generator.Interfaces;
using Reqnroll.Generator.Plugins;
using Reqnroll.Infrastructure;
using Reqnroll.UnitTestProvider;

[assembly:GeneratorPlugin(typeof(ExternalDataGeneratorPlugin))]

namespace Reqnroll.ExternalData.ReqnrollPlugin
{
    public class ExternalDataGeneratorPlugin : IGeneratorPlugin
    {
        public void Initialize(GeneratorPluginEvents generatorPluginEvents, GeneratorPluginParameters generatorPluginParameters,
            UnitTestProviderConfiguration unitTestProviderConfiguration)
        {
            generatorPluginEvents.RegisterDependencies += (sender, args) =>
            {
                args.ObjectContainer.RegisterTypeAs<ExternalDataTestGenerator, ITestGenerator>();
                
                args.ObjectContainer.RegisterTypeAs<SpecificationProvider, ISpecificationProvider>();
                args.ObjectContainer.RegisterTypeAs<DataSourceLoaderFactory, IDataSourceLoaderFactory>();
                args.ObjectContainer.RegisterTypeAs<CsvLoader, IDataSourceLoader>("CSV");
                args.ObjectContainer.RegisterTypeAs<ExcelLoader, IDataSourceLoader>("Excel");
                args.ObjectContainer.RegisterTypeAs<JsonLoader, IDataSourceLoader>("JSON");
            };
        }
    }
}
