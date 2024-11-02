namespace Reqnroll.FeatureSourceGenerator.SourceModel;
public class ParameterDescriptor : IEquatable<ParameterDescriptor?>
{
    public ParameterDescriptor(IdentifierString name, TypeIdentifier type)
    {
        if (name.IsEmpty)
        {
            throw new ArgumentException("Value cannot be an empty identifier.", nameof(name));
        }

        Name = name;
        Type = type;
    }

    public IdentifierString Name { get; }

    public TypeIdentifier Type { get; }

    public bool Equals(ParameterDescriptor? other)
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
            Type.Equals(other.Type);
    }

    public override bool Equals(object obj) => Equals(obj as ParameterDescriptor);

    public override int GetHashCode()
    {
        unchecked
        {
            var hash = 65362369;

            hash *= 45172373 + Name.GetHashCode();
            hash *= 45172373 + Type.GetHashCode();

            return hash;
        }
    }

    public override string ToString() => $"{Type} {Name}";
}
