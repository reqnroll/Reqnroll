using Reqnroll.Parser;

namespace Reqnroll.Generator.UnitTestConverter;

public interface IFeatureGeneratorRegistry
{
    IFeatureGenerator CreateGenerator(ReqnrollDocument document);
}