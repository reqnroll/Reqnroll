namespace Reqnroll.FeatureSourceGenerator.SourceModel;

public class SimpleTypeIdentifier(IdentifierString name, bool isNullable = false) : 
    LocalTypeIdentifier(isNullable), IEquatable<SimpleTypeIdentifier?>
{
    public IdentifierString Name { get; } = 
        name.IsEmpty ? throw new ArgumentException("Value cannot be an empty identifier.", nameof(name)) : name;

    public override bool Equals(object obj) => Equals(obj as SimpleTypeIdentifier);

    public bool Equals(SimpleTypeIdentifier? other)
    {
        if (other is null)
        {
            return false;
        }

        if (ReferenceEquals(this, other))
        {
            return true;
        }

        return Name.Equals(other.Name) &&
            IsNullable.Equals(other.IsNullable);
    }

    public static bool Equals(SimpleTypeIdentifier? typeA, SimpleTypeIdentifier? typeB)
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
            var hash = 62786819;

            hash *= 41806399 + Name.GetHashCode();
            hash *= 41806399 + IsNullable.GetHashCode();

            return hash;
        }
    }

    public override string ToString()
    {
        if (IsNullable)
        {
            return Name + '?';
        }
        else
        {
            return Name;
        }
    }

    public static bool operator ==(SimpleTypeIdentifier? typeA, SimpleTypeIdentifier? typeB) => Equals(typeA, typeB);

    public static bool operator !=(SimpleTypeIdentifier? typeA, SimpleTypeIdentifier? typeB) => !Equals(typeA, typeB);
}
