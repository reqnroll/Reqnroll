namespace Reqnroll.FeatureSourceGenerator.SourceModel;

public class ArrayTypeIdentifier(TypeIdentifier itemType, bool isNullable = false) : 
    TypeIdentifier, IEquatable<ArrayTypeIdentifier?>
{
    public TypeIdentifier ItemType { get; } = itemType;

    public override bool IsNullable { get; } = isNullable;

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

        return ItemType.Equals(other.ItemType) &&
            IsNullable.Equals(other.IsNullable);
    }

    public override bool Equals(object obj) => Equals(obj as ArrayTypeIdentifier);

    public override int GetHashCode()
    {
        unchecked
        {
            var hash = 36571313;

            hash *= 82795997 + ItemType.GetHashCode();
            hash *= 82795997 + IsNullable.GetHashCode();

            return hash;
        }
    }

    public override string ToString()
    {
        var str = $"{ItemType}[]";

        if (IsNullable)
        {
            str += '?';
        }

        return str;
    }

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
