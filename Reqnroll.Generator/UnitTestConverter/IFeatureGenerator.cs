using System.CodeDom;
using System.Collections.Generic;
using Reqnroll.Parser;

namespace Reqnroll.Generator.UnitTestConverter
{
    public interface IFeatureGenerator
    {
        UnitTestFeatureGenerationResult GenerateUnitTestFixture(ReqnrollDocument document, string testClassName, string targetNamespace);
    }
}