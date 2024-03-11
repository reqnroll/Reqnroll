using System.Collections.Generic;
using System.Linq;
using Reqnroll.BoDi;
using Reqnroll.Parser;

namespace Reqnroll.Generator.UnitTestConverter
{
    public class FeatureGeneratorRegistry : IFeatureGeneratorRegistry
    {
        private readonly List<IFeatureGeneratorProvider> providers;

        public FeatureGeneratorRegistry(IObjectContainer objectContainer)
        {
            providers = objectContainer.ResolveAll<IFeatureGeneratorProvider>().ToList().OrderBy(item => item.Priority).ToList();
        }

        public IFeatureGenerator CreateGenerator(ReqnrollDocument document)
        {
            var providerItem = FindProvider(document);
            return providerItem.CreateGenerator(document);
        }

        private IFeatureGeneratorProvider FindProvider(ReqnrollDocument feature)
        {
            return providers.First(item => item.CanGenerate(feature));
        }
    }
}