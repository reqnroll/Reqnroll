using Reqnroll.FeatureSourceGenerator.SourceModel;

namespace Reqnroll.FeatureSourceGenerator;

public readonly struct NamespaceString : IEquatable<NamespaceString>, IEquatable<string>
{
    private readonly string? _value;

    public static readonly NamespaceString Empty = default;

    public NamespaceString(string s)
    {
        if (string.IsNullOrEmpty(s))
        {
            _value = string.Empty;
            return;
        }

        var atStartOfIdentifier = true;

        foreach (var c in s)
        {
            if (atStartOfIdentifier)
            {
                if (!IdentifierString.IsValidAsFirstCharacterInIdentifier(c))
                {
                    throw new ArgumentException(
                        $"'{c}' cannot appear as the first character in an identifier.",
                        nameof(s));
                }

                atStartOfIdentifier = false;
            }
            else if (c == '.')
            {
                atStartOfIdentifier = true;
            }
            else
            {
                if (!IdentifierString.IsValidInIdentifier(c))
                {
                    throw new ArgumentException(
                        $"'{c}' cannot appear in an identifier.",
                        nameof(s));
                }
            }
        }

        if (atStartOfIdentifier)
        {
            throw new ArgumentException(
                $"'.' cannot appear as the last character in the namespace.",
                nameof(s));
        }

        _value = s;
    }

    public readonly bool IsEmpty => string.IsNullOrEmpty(_value);

    public override string ToString() => _value ?? "";

    public override int GetHashCode() => _value?.GetHashCode() ?? 0;

    public override bool Equals(object obj)
    {
        return obj switch
        {
            null => IsEmpty,
            NamespaceString ns => Equals(ns),
            string s => Equals(s),
            _ => false
        };
    }

    public bool Equals(string other)
    {
        if (string.IsNullOrEmpty(other))
        {
            return IsEmpty;
        }

        return _value!.Equals(other, StringComparison.Ordinal);
    }

    public bool Equals(NamespaceString other)
    {
        if (other.IsEmpty)
        {
            return IsEmpty;
        }

        return _value!.Equals(other._value, StringComparison.Ordinal);
    }

    public static bool operator ==(NamespaceString namespaceA, NamespaceString namespaceB) => Equals(namespaceA, namespaceB);

    public static bool operator !=(NamespaceString namespaceA, NamespaceString namespaceB) => !Equals(namespaceA, namespaceB);

    public static bool operator ==(NamespaceString ns, string s) => Equals(ns, s);

    public static bool operator !=(NamespaceString ns, string s) => !Equals(ns, s);

    public static implicit operator string(NamespaceString ns) => ns.ToString();
}
