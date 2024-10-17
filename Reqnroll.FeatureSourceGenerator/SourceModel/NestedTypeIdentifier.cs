namespace Reqnroll.FeatureSourceGenerator.SourceModel;

public class NestedTypeIdentifier(TypeIdentifier encapsulatingType, LocalTypeIdentifier localType) : 
    TypeIdentifier, IEquatable<NestedTypeIdentifier?>
{
    public TypeIdentifier EncapsulatingType { get; } = encapsulatingType;

    public LocalTypeIdentifier LocalType { get; } = localType;

    public override bool IsNullable => LocalType.IsNullable;

    public bool Equals(NestedTypeIdentifier? other)
    {
        if (other is null)
        {
            return false;
        }

        if (ReferenceEquals(this, other))
        {
            return true;
        }

        return EncapsulatingType.Equals(other.EncapsulatingType) &&
            LocalType.Equals(other.LocalType);
    }

    public override bool Equals(object obj) => Equals(obj as NestedTypeIdentifier);

    public override int GetHashCode()
    {
        unchecked
        {
            var hash = 69067723;

            hash *= 81073471 + EncapsulatingType.GetHashCode();
            hash *= 81073471 + LocalType.GetHashCode();

            return hash;
        }
    }
}
