﻿using System.Collections.Immutable;

namespace Reqnroll.FeatureSourceGenerator;

public abstract record CompilationInformation(
    string? AssemblyName,
    ImmutableArray<AssemblyIdentity> ReferencedAssemblies)
{
    public virtual bool Equals(CompilationInformation other)
    {
        if (other is null)
        {
            return false;
        }

        if (ReferenceEquals(this, other))
        {
            return true;
        }

        return string.Equals(AssemblyName, other.AssemblyName, StringComparison.Ordinal)
            && ReferencedAssemblies.SequenceEqual(other.ReferencedAssemblies);
    }

    public override int GetHashCode()
    {
        unchecked // Overflow is fine, just wrap
        {
            int hash = 27;

            hash = hash * 43 + AssemblyName?.GetHashCode() ?? 0;

            foreach (var assembly in ReferencedAssemblies)
            {
                hash = hash * 43 + assembly.GetHashCode();
            }

            return hash;
        }
    }
}
