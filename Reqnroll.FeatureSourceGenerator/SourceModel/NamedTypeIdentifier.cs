using Reqnroll.FeatureSourceGenerator.SourceModel;

namespace Reqnroll.FeatureSourceGenerator;

public class NamedTypeIdentifier : TypeIdentifier, IEquatable<NamedTypeIdentifier?>
{
    public NamedTypeIdentifier(IdentifierString localName) : this(NamespaceString.Empty, localName)
    {
    }

    public NamedTypeIdentifier(NamespaceString ns, IdentifierString localName, bool isNullable = false) : base(isNullable)
    {
        if (localName.IsEmpty && !ns.IsEmpty)
        {
            throw new ArgumentException(
                "An empty local name cannot be combined with a non-empty namespace.",
                nameof(localName));
        }

        LocalName = localName;
        Namespace = ns;
    }

    public IdentifierString LocalName { get; }

    public NamespaceString Namespace { get; }

    public override string? ToString() => Namespace.IsEmpty ? LocalName.ToString() : $"{Namespace}.{LocalName}";

    public override bool Equals(object obj) => Equals(obj as NamedTypeIdentifier);

    public override bool Equals(TypeIdentifier? other) => Equals(other as NamedTypeIdentifier);

    public bool Equals(NamedTypeIdentifier? other)
    {
        if (other is null)
        {
            return false;
        }

        if (ReferenceEquals(this, other))
        {
            return true;
        }

        return base.Equals(other) && 
            Namespace.Equals(other.Namespace) && 
            LocalName.Equals(other.LocalName);
    }

    public override int GetHashCode()
    {
        unchecked
        {
            var hashCode = base.GetHashCode();
            hashCode *= 73812971 + LocalName.GetHashCode();
            hashCode *= 73812971 + Namespace.GetHashCode();
            return hashCode;
        }
    }

    public static bool Equals(NamedTypeIdentifier? first, NamedTypeIdentifier? second)
    {
        if (ReferenceEquals(first, second))
        {
            return true;
        }

        if (first is null)
        {
            return false;
        }

        return first.Equals(second);
    }

    public static bool operator ==(NamedTypeIdentifier? first, NamedTypeIdentifier? second) => Equals(first, second);

    public static bool operator !=(NamedTypeIdentifier? first, NamedTypeIdentifier? second) => !Equals(first, second);
}
