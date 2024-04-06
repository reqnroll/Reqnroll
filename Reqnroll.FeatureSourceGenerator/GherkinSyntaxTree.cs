using Gherkin.Ast;
using Microsoft.CodeAnalysis;
using System.Collections.Immutable;

namespace Reqnroll.FeatureSourceGenerator;

public class GherkinSyntaxTree : IEquatable<GherkinSyntaxTree>
{
    private readonly GherkinDocument _root;
    private readonly ImmutableArray<Diagnostic> _diagnostics;

    internal GherkinSyntaxTree(GherkinDocument root, ImmutableArray<Diagnostic> diagnostics, string? path )
    {
        _root = root;
        _diagnostics = diagnostics;
        FilePath = path;
    }

    public string? FilePath { get; }

    public GherkinSyntaxTree WithPath(string? path) => new(_root, _diagnostics, path);

    public override bool Equals(object obj) => obj is GherkinSyntaxTree tree && Equals(tree);

    public bool Equals(GherkinSyntaxTree other)
    {
        if (other is null)
        {
            return false;
        }

        if (ReferenceEquals(this, other))
        {
            return true;
        }

        return _diagnostics.SetEquals(other._diagnostics) && GherkinDocumentComparer.Default.Equals(_root, other._root);
    }

    public override int GetHashCode()
    {
        const int multiplier = 31;

        var hash = 17;

        hash *= multiplier + GherkinDocumentComparer.Default.GetHashCode(_root);

        foreach (var diagnostic in _diagnostics)
        {
            hash *= multiplier + diagnostic.GetHashCode();
        }

        return hash;
    }

    public ImmutableArray<Diagnostic> GetDiagnostics() => _diagnostics;

    public GherkinDocument GetRoot() => _root;
}
