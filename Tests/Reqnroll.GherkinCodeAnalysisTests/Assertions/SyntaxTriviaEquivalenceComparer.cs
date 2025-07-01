using Reqnroll.CodeAnalysis.Gherkin.Syntax;
using System.Diagnostics.CodeAnalysis;

namespace Reqnroll.CodeAnalysis.Gherkin.Assertions;

public class SyntaxTriviaEquivalenceComparer : IEqualityComparer<SyntaxTrivia>
{
    public bool Equals(SyntaxTrivia x, SyntaxTrivia y) => x.IsEquivalentTo(y);

    public int GetHashCode([DisallowNull] SyntaxTrivia obj) => obj.GetHashCode();
}
