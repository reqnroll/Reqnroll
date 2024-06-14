using System.Collections.Immutable;

namespace Reqnroll.FeatureSourceGenerator;

internal record TestFixtureComposition(FeatureInformation Feature, ImmutableArray<TestMethod> Methods)
{
    public override int GetHashCode()
    {
        unchecked
        {
            var hash = 49151149;

            hash *= 983819 + Feature.GetHashCode();
            hash *= 983819 + Methods.GetSetHashCode();

            return hash;
        }
    }

    public virtual bool Equals(TestFixtureComposition? other)
    {
        if (other is null)
        {
            return false;
        }

        return Feature.Equals(other.Feature) &&
            (Methods.Equals(other.Methods) || Methods.SetEqual(other.Methods));
    }
}
