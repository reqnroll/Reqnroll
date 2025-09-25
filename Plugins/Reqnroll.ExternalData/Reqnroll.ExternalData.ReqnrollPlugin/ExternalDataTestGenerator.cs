using System.IO;
using Reqnroll.ExternalData.ReqnrollPlugin.Transformation;
using Reqnroll.Configuration;
using Reqnroll.Generator;
using Reqnroll.Generator.CodeDom;
using Reqnroll.Generator.Configuration;
using Reqnroll.Generator.Interfaces;
using Reqnroll.Generator.UnitTestConverter;
using Reqnroll.Parser;

namespace Reqnroll.ExternalData.ReqnrollPlugin;

public class ExternalDataTestGenerator(
    ReqnrollConfiguration reqnrollConfiguration,
    ProjectSettings projectSettings,
    IFeatureGeneratorRegistry featureGeneratorRegistry,
    CodeDomHelper codeDomHelper,
    IGherkinParserFactory gherkinParserFactory,
    GeneratorInfo generatorInfo,
    IncludeExternalDataTransformation includeExternalDataTransformation)
    : TestGenerator(reqnrollConfiguration, projectSettings, featureGeneratorRegistry, codeDomHelper, gherkinParserFactory, generatorInfo)
{
    protected override ReqnrollDocument ParseContent(IGherkinParser parser, TextReader contentReader,
        ReqnrollDocumentLocation documentLocation)
    {
        var document = base.ParseContent(parser, contentReader, documentLocation);
        document = includeExternalDataTransformation.TransformDocument(document);
        return document; 
    }
}