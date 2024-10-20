using System.Collections.Immutable;
using System.Text;

namespace Reqnroll.FeatureSourceGenerator.SourceModel;

public class GenericTypeIdentifier(
    IdentifierString name, ImmutableArray<TypeIdentifier>
    typeArguments,
    bool isNullable = false) : LocalTypeIdentifier(isNullable), IEquatable<GenericTypeIdentifier?>
{
    public IdentifierString Name { get; } =
        name.IsEmpty ? throw new ArgumentException("Value cannot be an empty identifier.", nameof(name)) : name;

    public ImmutableArray<TypeIdentifier> TypeArguments { get; } = 
        typeArguments.IsDefaultOrEmpty ? 
        throw new ArgumentException("Value cannot be an empty array.", nameof(typeArguments)) : 
        typeArguments;

    public override bool Equals(object obj) => Equals(obj as GenericTypeIdentifier);

    public bool Equals(GenericTypeIdentifier? other)
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
            IsNullable.Equals(other.IsNullable) &&
            TypeArguments.SequenceEqual(other.TypeArguments);
    }

    public static bool Equals(GenericTypeIdentifier? typeA, GenericTypeIdentifier? typeB)
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
            var hash = 42650617;

            hash *= 57433771 + Name.GetHashCode();
            hash *= 57433771 + IsNullable.GetHashCode();
            hash *= 57433771 + TypeArguments.GetSequenceHashCode();

            return hash;
        }
    }

    public override string ToString()
    {
        var sb = new StringBuilder(Name);

        sb.Append('<');

        sb.Append(TypeArguments[0].ToString());

        for (var i = 1; i < TypeArguments.Length; i++)
        {
            sb.Append(',');
            sb.Append(TypeArguments[i].ToString());
        }
        
        sb.Append('>');

        return sb.ToString();
    }

    public static bool operator ==(GenericTypeIdentifier? typeA, GenericTypeIdentifier? typeB) => Equals(typeA, typeB);

    public static bool operator !=(GenericTypeIdentifier? typeA, GenericTypeIdentifier? typeB) => !Equals(typeA, typeB);
}
