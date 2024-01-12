using Reqnroll.Generator.Generation;
using Reqnroll.Parser;

namespace Reqnroll.Generator.UnitTestConverter
{
    public class UnitTestFeatureGeneratorProvider : IFeatureGeneratorProvider
    {
        private readonly UnitTestFeatureGenerator _unitTestFeatureGenerator;

        public UnitTestFeatureGeneratorProvider(UnitTestFeatureGenerator unitTestFeatureGenerator)
        {
            _unitTestFeatureGenerator = unitTestFeatureGenerator;
        }

        public int Priority => PriorityValues.Lowest;

        public bool CanGenerate(ReqnrollDocument document)
        {
            return true;
        }

        public IFeatureGenerator CreateGenerator(ReqnrollDocument document)
        {
            return _unitTestFeatureGenerator;
        }
    }
}