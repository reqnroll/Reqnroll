namespace Reqnroll.FeatureSourceGenerator;

public readonly struct TypeIdentifier : IEquatable<TypeIdentifier>, IEquatable<string?>
{
    public TypeIdentifier(IdentifierString localName) : this(NamespaceString.Empty, localName)
    {
    }

    public TypeIdentifier(NamespaceString ns, IdentifierString localName)
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

    public readonly bool IsEmpty => LocalName.IsEmpty;

    public IdentifierString LocalName { get; }

    public NamespaceString Namespace { get; }

    public override string? ToString() => Namespace.IsEmpty ? LocalName.ToString() : $"{Namespace}.{LocalName}";

    public static bool Equals(TypeIdentifier identifierA, TypeIdentifier identifierB)
    {
        return identifierA.Equals(identifierB);
    }

    public static bool Equals(TypeIdentifier identifier, string? s) => identifier.Equals(s);

    public override bool Equals(object obj)
    {
        return obj switch
        {
            null => IsEmpty,
            TypeIdentifier id => Equals(id),
            string s => Equals(s),
            _ => false
        };
    }

    public bool Equals(TypeIdentifier other)
    {
        if (IsEmpty)
        {
            return other.IsEmpty;
        }

        return Namespace.Equals(other.Namespace) && LocalName.Equals(other.LocalName);
    }

    public bool Equals(string? other)
    {
        if (string.IsNullOrEmpty(other))
        {
            return IsEmpty;
        }

        if (Namespace.IsEmpty)
        {
            return LocalName.Equals(other);
        }

        var ns = Namespace.ToString();
        var ln = LocalName.ToString();

        if (ln.Length + ns.Length + 1 != other!.Length)
        {
            return false;
        }

        var otherSpan = other.AsSpan();
        return otherSpan.Slice(0, ns.Length).Equals(ns.AsSpan(), StringComparison.Ordinal) &&
            otherSpan[ns.Length].Equals('.') &&
            otherSpan.Slice(ns.Length + 1).Equals(ln.AsSpan(), StringComparison.Ordinal);
    }

    public override int GetHashCode()
    {
        unchecked
        {
            var hashCode = 1922063139;
            hashCode *= -1521134295 + LocalName.GetHashCode();
            hashCode *= -1521134295 + Namespace.GetHashCode();
            return hashCode;
        }
    }

    public static bool operator ==(TypeIdentifier identifierA, TypeIdentifier identifierB) => Equals(identifierA, identifierB);

    public static bool operator !=(TypeIdentifier identifierA, TypeIdentifier identifierB) => !Equals(identifierA, identifierB);

    public static bool operator ==(TypeIdentifier identifier, string s) => Equals(identifier, s);

    public static bool operator !=(TypeIdentifier identifier, string s) => !Equals(identifier, s);
}
