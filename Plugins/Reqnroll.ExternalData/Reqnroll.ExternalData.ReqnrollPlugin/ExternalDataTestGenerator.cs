using System.IO;
using Reqnroll.ExternalData.ReqnrollPlugin.Transformation;
using Reqnroll.Configuration;
using Reqnroll.Generator;
using Reqnroll.Generator.CodeDom;
using Reqnroll.Generator.Configuration;
using Reqnroll.Generator.Interfaces;
using Reqnroll.Generator.UnitTestConverter;
using Reqnroll.Parser;

namespace Reqnroll.ExternalData.ReqnrollPlugin
{
    public class ExternalDataTestGenerator : TestGenerator
    {
        private readonly IncludeExternalDataTransformation _includeExternalDataTransformation;

        public ExternalDataTestGenerator(ReqnrollConfiguration reqnrollConfiguration, ProjectSettings projectSettings, ITestHeaderWriter testHeaderWriter, ITestUpToDateChecker testUpToDateChecker, IFeatureGeneratorRegistry featureGeneratorRegistry, CodeDomHelper codeDomHelper, IGherkinParserFactory gherkinParserFactory, GeneratorInfo generatorInfo, IncludeExternalDataTransformation includeExternalDataTransformation) 
            : base(reqnrollConfiguration, projectSettings, testHeaderWriter, testUpToDateChecker, featureGeneratorRegistry, codeDomHelper, gherkinParserFactory, generatorInfo)
        {
            _includeExternalDataTransformation = includeExternalDataTransformation;
        }

        protected override ReqnrollDocument ParseContent(IGherkinParser parser, TextReader contentReader,
            ReqnrollDocumentLocation documentLocation)
        {
            var document = base.ParseContent(parser, contentReader, documentLocation);
            document = _includeExternalDataTransformation.TransformDocument(document);
            return document; 
        }
    }
}
