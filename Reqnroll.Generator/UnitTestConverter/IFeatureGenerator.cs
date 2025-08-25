using System.CodeDom;
using System.Collections.Generic;
using Reqnroll.Parser;

namespace Reqnroll.Generator.UnitTestConverter
{
    public interface IFeatureGenerator
    {
        CodeNamespace GenerateUnitTestFixture(ReqnrollDocument document, string testClassName, string targetNamespace, out IEnumerable<string> warnings, bool featureFilesEmbedded = false);
    }
}