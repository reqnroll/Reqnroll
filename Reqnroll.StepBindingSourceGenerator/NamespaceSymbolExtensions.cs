using Microsoft.CodeAnalysis;

namespace Reqnroll.StepBindingSourceGenerator;

internal static class NamespaceSymbolExtensions
{
    public static Namespace ToNamespaceRecord(this INamespaceSymbol symbol)
    {
        var parent = symbol.ContainingNamespace == null || symbol.ContainingNamespace.IsGlobalNamespace ?
            null : 
            symbol.ContainingNamespace.ToNamespaceRecord();

        return new Namespace(symbol.Name, parent);
    }
}
