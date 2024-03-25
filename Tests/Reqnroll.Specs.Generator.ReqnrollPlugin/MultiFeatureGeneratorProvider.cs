using System.Collections.Generic;
using Reqnroll.BoDi;
using Reqnroll.Configuration;
using Reqnroll.Generator.CodeDom;
using Reqnroll.Generator.Interfaces;
using Reqnroll.Generator.UnitTestConverter;
using Reqnroll.Parser;

namespace Reqnroll.Specs.Generator.ReqnrollPlugin
{
    internal class MultiFeatureGeneratorProvider : IFeatureGeneratorProvider
    {
        

        private readonly MultiFeatureGenerator _multiFeatureGenerator;

        public MultiFeatureGeneratorProvider(IObjectContainer container)
        {
            var featureGenerators = new List<KeyValuePair<Combination, IFeatureGenerator>>();

            foreach (var combination in TestRunCombinations.List)
            {
                var combinationFeatureGenerator = new CombinationFeatureGenerator(container.Resolve<CodeDomHelper>(), container.Resolve<ReqnrollConfiguration>(), container.Resolve<IDecoratorRegistry>(), combination, container.Resolve<ProjectSettings>());
                featureGenerators.Add(new KeyValuePair<Combination, IFeatureGenerator>(combination, combinationFeatureGenerator));
            }

            _multiFeatureGenerator = new MultiFeatureGenerator(featureGenerators, new CombinationFeatureGenerator(container.Resolve<CodeDomHelper>(), container.Resolve<ReqnrollConfiguration>(), container.Resolve<IDecoratorRegistry>(), null, container.Resolve<ProjectSettings>()));
        }


        public bool CanGenerate(ReqnrollDocument reqnrollDocument)
        {
            return true;
        }

        public IFeatureGenerator CreateGenerator(ReqnrollDocument reqnrollDocument)
        {
            return _multiFeatureGenerator;
        }

        public int Priority => PriorityValues.Normal;
    }
}