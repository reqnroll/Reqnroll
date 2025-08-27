using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Text;

namespace Reqnroll.Generator.Interfaces;

public class UnitTestFeatureGenerationResult
{
    public CodeNamespace CodeNameSpace { get; }
    public string FeatureMessages { get; }
    public IEnumerable<string> GenerationWarnings { get; }

    public UnitTestFeatureGenerationResult(CodeNamespace codeNameSpace, string featureMessages, IEnumerable<string> generationWarnings)
    {
        CodeNameSpace = codeNameSpace;
        FeatureMessages = featureMessages;
        GenerationWarnings = generationWarnings;
    }
}
