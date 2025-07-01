using Reqnroll.CodeAnalysis.Gherkin.Syntax;
using System.Diagnostics.CodeAnalysis;

namespace Reqnroll.CodeAnalysis.Gherkin.Assertions;

public class SyntaxTokenEquivalenceComparer : IEqualityComparer<SyntaxToken>
{
    public bool Equals(SyntaxToken x, SyntaxToken y) => x.IsEquivalentTo(y);

    public int GetHashCode([DisallowNull] SyntaxToken obj) => obj.GetHashCode();
}
