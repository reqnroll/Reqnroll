namespace Reqnroll.FeatureSourceGenerator.SourceModel;

public class QualifiedTypeIdentifier(NamespaceString ns, LocalTypeIdentifier localType) : 
    TypeIdentifier, IEquatable<QualifiedTypeIdentifier?>
{
    public NamespaceString Namespace { get; } = 
        ns.IsEmpty ? throw new ArgumentException("Value cannot be an empty namespace.", nameof(ns)) : ns;

    public LocalTypeIdentifier LocalType { get; } = localType;

    public override bool IsNullable => LocalType.IsNullable;

    public bool Equals(QualifiedTypeIdentifier? other)
    {
        if (other is null)
        {
            return false;
        }

        if (ReferenceEquals(this, other))
        {
            return true;
        }

        return Namespace.Equals(other.Namespace) &&
            LocalType.Equals(other.LocalType) &&
            IsNullable.Equals(other.IsNullable);
    }

    public override bool Equals(object obj) => Equals(obj as QualifiedTypeIdentifier);

    public static bool Equals(QualifiedTypeIdentifier? typeA, QualifiedTypeIdentifier? typeB)
    {
        if (ReferenceEquals(typeA, typeB))
        {
            return true;
        }

        if (typeA is null)
        {
            return false;
        }

        return typeA.Equals(typeB);
    }

    public override int GetHashCode()
    {
        unchecked
        {
            var hash = 20462011;

            hash *= 50825449 + Namespace.GetHashCode();
            hash *= 50825449 + LocalType.GetHashCode();
            hash *= 50825449 + IsNullable.GetHashCode();

            return hash;
        }
    }

    public override string ToString() => $"{Namespace}.{LocalType}";

    public static bool operator ==(QualifiedTypeIdentifier? typeA, QualifiedTypeIdentifier? typeB) => Equals(typeA, typeB);

    public static bool operator !=(QualifiedTypeIdentifier? typeA, QualifiedTypeIdentifier? typeB) => !Equals(typeA, typeB);
}
