namespace Reqnroll.FeatureSourceGenerator;

public class ArrayTypeIdentifier(TypeIdentifier itemType, bool isNullable = false) : 
    TypeIdentifier(isNullable), IEquatable<ArrayTypeIdentifier?>
{
    public TypeIdentifier ItemType { get; } = itemType;

    public bool Equals(ArrayTypeIdentifier? other)
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
            ItemType.Equals(other.ItemType);
    }

    public override bool Equals(object obj) => Equals(obj as ArrayTypeIdentifier);

    public override bool Equals(TypeIdentifier? other) => Equals(other as ArrayTypeIdentifier);

    public override int GetHashCode()
    {
        unchecked
        {
            var hash = base.GetHashCode();

            hash *= ItemType.GetHashCode();

            return hash;
        }
    }

    public override string ToString() => $"{ItemType}[]";

    public static bool Equals(ArrayTypeIdentifier? first, ArrayTypeIdentifier? second)
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

    public static bool operator ==(ArrayTypeIdentifier? first, ArrayTypeIdentifier? second) => Equals(first, second);

    public static bool operator !=(ArrayTypeIdentifier? first, ArrayTypeIdentifier? second) => !Equals(first, second);
}
