using Reqnroll.Bindings.Reflection;

namespace Reqnroll.Bindings.CucumberExpressions;

public interface ICucumberExpressionParameterTypeTransformation
{
    string Name { get; }
    string Regex { get; }
    IBindingType TargetType { get; }
    bool UseForSnippets { get; }
    int Weight { get; }
}
