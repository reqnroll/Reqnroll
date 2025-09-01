using System.CodeDom;
using System.Collections.Generic;

namespace Reqnroll.Generator.UnitTestConverter;

public class UnitTestFeatureGenerationResult(CodeNamespace codeNamespace, string featureMessages, IEnumerable<string> generationWarnings)
{
    public CodeNamespace CodeNamespace { get; } = codeNamespace;

    public string FeatureMessages { get; } = featureMessages;

    public IEnumerable<string> GenerationWarnings { get; } = generationWarnings;
}
