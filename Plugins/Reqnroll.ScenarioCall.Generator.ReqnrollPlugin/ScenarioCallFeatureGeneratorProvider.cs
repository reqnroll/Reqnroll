using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Reqnroll.Generator.UnitTestConverter;
using Reqnroll.Parser;

namespace Reqnroll.ScenarioCall.Generator.ReqnrollPlugin
{
    public class ScenarioCallFeatureGeneratorProvider : IFeatureGeneratorProvider
    {
        private readonly IFeatureGeneratorProvider _baseProvider;

        public ScenarioCallFeatureGeneratorProvider(UnitTestFeatureGeneratorProvider baseProvider)
        {
            _baseProvider = baseProvider;
        }

        public int Priority => PriorityValues.High; // Higher priority than base provider

        public bool CanGenerate(ReqnrollDocument document)
        {
            return _baseProvider.CanGenerate(document);
        }

        public IFeatureGenerator CreateGenerator(ReqnrollDocument document)
        {
            var baseGenerator = _baseProvider.CreateGenerator(document);
            return new ScenarioCallFeatureGenerator(baseGenerator, document);
        }
    }
}