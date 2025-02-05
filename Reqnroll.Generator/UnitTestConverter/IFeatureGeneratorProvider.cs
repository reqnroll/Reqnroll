using Reqnroll.Parser;

namespace Reqnroll.Generator.UnitTestConverter
{
    public interface IFeatureGeneratorProvider
    {
        int Priority { get; }
        bool CanGenerate(ReqnrollDocument document);
        IFeatureGenerator CreateGenerator(ReqnrollDocument document);
    }
}