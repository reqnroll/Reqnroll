namespace Reqnroll.FeatureSourceGenerator;

public abstract class TypeIdentifier(bool isNullable) : IEquatable<TypeIdentifier?>
{
    public bool IsNullable { get; } = isNullable;

    public virtual bool Equals(TypeIdentifier? other)
    {
        if (other is null)
        {
            return false;
        }

        if (ReferenceEquals(this, other))
        {
            return true;
        }

        return IsNullable.Equals(other.IsNullable);
    }

    public override bool Equals(object obj) => Equals(obj as TypeIdentifier);

    public override int GetHashCode() => IsNullable.GetHashCode();

    public static bool Equals(TypeIdentifier? first, TypeIdentifier? second)
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

    public static bool operator ==(TypeIdentifier? first, TypeIdentifier? second) => Equals(first, second);

    public static bool operator !=(TypeIdentifier? first, TypeIdentifier? second) => !Equals(first, second);
}
