using Reqnroll.BoDi;
using Reqnroll.Configuration;
using Reqnroll.Generator.Generation;
using Reqnroll.Generator.Interfaces;
using Reqnroll.Generator.Plugins;
using Reqnroll.Generator.UnitTestConverter;
using Reqnroll.Parser;
using Reqnroll.PlatformCompatibility;
using Reqnroll.Tracing;
using Reqnroll.Utils;

namespace Reqnroll.Generator
{
    internal partial class DefaultDependencyProvider
    {
        partial void RegisterUnitTestGeneratorProviders(ObjectContainer container);

        public virtual void RegisterDefaults(ObjectContainer container)
        {
            container.RegisterTypeAs<FileSystem, IFileSystem>();

            container.RegisterTypeAs<TestGenerator, ITestGenerator>();

#pragma warning disable CS0618 // Type or member is obsolete
            // Interfaces to be removed in v4
            container.RegisterTypeAs<ObsoleteTestHeaderWriter, ITestHeaderWriter>();
            container.RegisterTypeAs<ObsoleteTestUpToDateChecker, ITestUpToDateChecker>();
#pragma warning restore CS0618 // Type or member is obsolete

            PlatformHelper.RegisterPluginAssemblyLoader(container);
            container.RegisterTypeAs<GeneratorPluginLoader, IGeneratorPluginLoader>();
            container.RegisterTypeAs<DefaultListener, ITraceListener>();

            container.RegisterTypeAs<UnitTestFeatureGenerator, UnitTestFeatureGenerator>();
            container.RegisterTypeAs<FeatureGeneratorRegistry, IFeatureGeneratorRegistry>();
            container.RegisterTypeAs<UnitTestFeatureGeneratorProvider, IFeatureGeneratorProvider>("default");
            container.RegisterTypeAs<TagFilterMatcher, ITagFilterMatcher>();

            container.RegisterTypeAs<DecoratorRegistry, IDecoratorRegistry>();
            container.RegisterTypeAs<IgnoreDecorator, ITestClassTagDecorator>("ignore");
            container.RegisterTypeAs<IgnoreDecorator, ITestMethodTagDecorator>("ignore");
            container.RegisterTypeAs<NonParallelizableDecorator, ITestClassTagDecorator>("nonparallelizable");
            container.RegisterTypeAs<NonParallelizableDecorator, ITestMethodTagDecorator>("nonparallelizable");

            container.RegisterInstanceAs(GenerationTargetLanguage.CreateCodeDomHelper(GenerationTargetLanguage.CSharp), GenerationTargetLanguage.CSharp, dispose: true);
            container.RegisterInstanceAs(GenerationTargetLanguage.CreateCodeDomHelper(GenerationTargetLanguage.VB), GenerationTargetLanguage.VB, dispose: true);

            container.RegisterTypeAs<ConfigurationLoader, IConfigurationLoader>();

            container.RegisterTypeAs<ReqnrollGherkinParserFactory, IGherkinParserFactory>();
            
            container.RegisterTypeAs<ReqnrollJsonLocator, IReqnrollJsonLocator>();

            RegisterUnitTestGeneratorProviders(container);
        }
    }
}
