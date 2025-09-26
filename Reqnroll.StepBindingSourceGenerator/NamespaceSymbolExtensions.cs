using Microsoft.CodeAnalysis;

namespace Reqnroll.StepBindingSourceGenerator;
internal static class NamespaceSymbolExtensions
{
    public static Namespace ToNamespaceRecord(this INamespaceSymbol symbol) => 
        new(symbol.Name, symbol.ContainingNamespace?.ToNamespaceRecord() ?? null);
}
