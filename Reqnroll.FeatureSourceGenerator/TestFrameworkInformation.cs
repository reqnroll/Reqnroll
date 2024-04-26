using System.Collections.Immutable;

namespace Reqnroll.FeatureSourceGenerator;

internal record TestFrameworkInformation(
    ImmutableArray<ITestFrameworkHandler> CompatibleHandlers,
    ImmutableArray<ITestFrameworkHandler> ReferencedHandlers,
    ITestFrameworkHandler? DefaultHandler)
{
    public override int GetHashCode()
    {
        unchecked
        {
            var hash = 47;

            hash *= 13 + CompatibleHandlers.GetSetHashCode();
            hash *= 13 + ReferencedHandlers.GetSetHashCode();
            hash *= 13 + DefaultHandler?.GetHashCode() ?? 0;

            return hash;
        }
    }
}
