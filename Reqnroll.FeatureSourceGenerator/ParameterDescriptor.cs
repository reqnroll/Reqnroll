using System;
using System.Collections.Generic;
using System.Text;

namespace Reqnroll.FeatureSourceGenerator;
public class ParameterDescriptor: IEquatable<ParameterDescriptor>
{
    public ParameterDescriptor(string name)
    {
        if (string.IsNullOrEmpty(name))
        {
            throw new ArgumentException("Value cannot be null or an empty string.", nameof(name));
        }

        Name = name;
    }

    public string Name { get; }

    public bool Equals(ParameterDescriptor? other)
    {
        if (ReferenceEquals(this, other))
        {
            return true;
        }

        if (other is null)
        {
            return false;
        }

        return Name.Equals(other.Name);
    }

    public override bool Equals(object obj) => Equals(obj as ParameterDescriptor);

    public override int GetHashCode()
    {
        return base.GetHashCode();
    }
}
