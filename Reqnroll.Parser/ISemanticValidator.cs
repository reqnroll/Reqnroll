using System.Collections.Generic;

namespace Reqnroll.Parser
{
    internal interface ISemanticValidator
    {
        List<SemanticParserException> Validate(ReqnrollFeature feature);
    }
}