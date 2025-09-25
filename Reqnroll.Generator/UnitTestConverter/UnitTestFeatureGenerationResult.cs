using System.CodeDom;
using System.Collections.Generic;

namespace Reqnroll.Generator.UnitTestConverter;

public class UnitTestFeatureGenerationResult(CodeNamespace codeNamespace, string featureMessages, string featureMessagesResourceName, IEnumerable<string> generationWarnings)
{
    public CodeNamespace CodeNamespace { get; } = codeNamespace;

    public string FeatureMessages { get; } = featureMessages;

    public string FeatureMessagesResourceName { get; } = featureMessagesResourceName;

    public IEnumerable<string> GenerationWarnings { get; } = generationWarnings;
}
