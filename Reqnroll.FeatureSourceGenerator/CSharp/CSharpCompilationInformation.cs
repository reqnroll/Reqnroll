using Microsoft.CodeAnalysis.CSharp;
using System.Collections.Immutable;

namespace Reqnroll.FeatureSourceGenerator.CSharp;

public record CSharpCompilationInformation(
    string? AssemblyName,
    ImmutableArray<AssemblyIdentity> ReferencedAssemblies,
    LanguageVersion LanguageVersion,
    bool HasNullableReferencesEnabled) : CompilationInformation(AssemblyName, ReferencedAssemblies)
{
    public virtual bool Equals(CSharpCompilationInformation other)
    {
        if (!base.Equals(other))
        {
            return false;
        }

        return LanguageVersion.Equals(other.LanguageVersion) &&
            HasNullableReferencesEnabled.Equals(other.HasNullableReferencesEnabled);
    }

    public override int GetHashCode()
    {
        unchecked // Overflow is fine, just wrap
        {
            int hash = base.GetHashCode();

            hash = hash * 43 + LanguageVersion.GetHashCode();
            hash = hash * 43 + HasNullableReferencesEnabled.GetHashCode();

            return hash;
        }
    }
}
