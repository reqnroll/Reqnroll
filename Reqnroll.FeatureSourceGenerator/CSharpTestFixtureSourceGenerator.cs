using Microsoft.CodeAnalysis;
using System.Collections.Immutable;

namespace Reqnroll.FeatureSourceGenerator;

[Generator(LanguageNames.CSharp)]
public class CSharpTestFixtureSourceGenerator : TestFixtureSourceGenerator
{
    public CSharpTestFixtureSourceGenerator()
    {
    }

    internal CSharpTestFixtureSourceGenerator(ImmutableArray<ITestFrameworkHandler> handlers) : base(handlers)
    {
    }
}
