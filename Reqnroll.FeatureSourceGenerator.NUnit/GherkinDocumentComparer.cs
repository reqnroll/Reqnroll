using Gherkin.Ast;

namespace Reqnroll.FeatureSourceGenerator.NUnit;

internal class GherkinDocumentComparer : IEqualityComparer<GherkinDocument>
{
    public static GherkinDocumentComparer Default { get; } = new GherkinDocumentComparer();

    public bool Equals(GherkinDocument x, GherkinDocument y)
    {
        return false;
    }

    public int GetHashCode(GherkinDocument obj)
    {
        if (obj == null)
        {
            return 0;
        }

        return obj.GetHashCode();
    }
}
