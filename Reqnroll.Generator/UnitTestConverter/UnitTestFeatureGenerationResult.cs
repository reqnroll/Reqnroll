using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Text;

namespace Reqnroll.Generator.UnitTestConverter;

public class UnitTestFeatureGenerationResult
{
    public CodeNamespace CodeNamespace { get; }
    public string FeatureMessages { get; }
    public IEnumerable<string> GenerationWarnings { get; }

    public UnitTestFeatureGenerationResult(CodeNamespace codeNamespace, string featureMessages, IEnumerable<string> generationWarnings)
    {
        CodeNamespace = codeNamespace;
        FeatureMessages = featureMessages;
        GenerationWarnings = generationWarnings;
    }
}
