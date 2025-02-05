using System.CodeDom;
using Reqnroll.Parser;

namespace Reqnroll.Generator.UnitTestConverter
{
    public interface IFeatureGenerator
    {
        CodeNamespace GenerateUnitTestFixture(ReqnrollDocument document, string testClassName, string targetNamespace);
    }
}