using System.Collections.Immutable;
using Reqnroll.FeatureSourceGenerator.SourceModel;

namespace Reqnroll.FeatureSourceGenerator;

internal record TestFixtureComposition<TCompilationInformation>(
    TestFixtureGenerationContext<TCompilationInformation> Context, 
    ImmutableArray<TestMethod> Methods)
    where TCompilationInformation : CompilationInformation
{
    public override int GetHashCode()
    {
        unchecked
        {
            var hash = 49151149;

            hash *= 983819 + Context.GetHashCode();
            hash *= 983819 + Methods.GetSetHashCode();

            return hash;
        }
    }

    public virtual bool Equals(TestFixtureComposition<TCompilationInformation>? other)
    {
        if (other is null)
        {
            return false;
        }

        return Context.Equals(other.Context) &&
            (Methods.Equals(other.Methods) || Methods.SetEqual(other.Methods));
    }
}
