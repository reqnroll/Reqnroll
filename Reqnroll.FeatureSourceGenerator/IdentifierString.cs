using System.Globalization;

namespace Reqnroll.FeatureSourceGenerator;

public readonly struct IdentifierString : IEquatable<IdentifierString>, IEquatable<string?>
{
    private readonly string? _value;

    public IdentifierString(string? s)
    {
        if (string.IsNullOrEmpty(s))
        {
            _value = null;
            return;
        }

        var atStartOfIdentifier = true;

        foreach (var c in s!)
        {
            if (atStartOfIdentifier)
            {
                if (!IsValidAsFirstCharacterInIdentifier(c))
                {
                    throw new ArgumentException(
                        $"'{c}' cannot appear as the first character in an identifier.",
                        nameof(s));
                }

                atStartOfIdentifier = false;
            }
            else
            {
                if (!IsValidInIdentifier(c))
                {
                    throw new ArgumentException(
                        $"'{c}' cannot appear in an identifier.",
                        nameof(s));
                }
            }
        }

        _value = s;
    }

    public readonly bool IsEmpty => _value == null;

    internal static bool IsValidAsFirstCharacterInIdentifier(char c)
    {
        if (c == '_')
        {
            return true;
        }

        var category = char.GetUnicodeCategory(c);

        return category == UnicodeCategory.UppercaseLetter
            || category == UnicodeCategory.LowercaseLetter
            || category == UnicodeCategory.TitlecaseLetter
            || category == UnicodeCategory.ModifierLetter
            || category == UnicodeCategory.OtherLetter;
    }

    internal static bool IsValidInIdentifier(char c)
    {
        var category = char.GetUnicodeCategory(c);

        return category == UnicodeCategory.UppercaseLetter
            || category == UnicodeCategory.LowercaseLetter
            || category == UnicodeCategory.TitlecaseLetter
            || category == UnicodeCategory.ModifierLetter
            || category == UnicodeCategory.OtherLetter
            || category == UnicodeCategory.LetterNumber
            || category == UnicodeCategory.NonSpacingMark
            || category == UnicodeCategory.SpacingCombiningMark
            || category == UnicodeCategory.DecimalDigitNumber
            || category == UnicodeCategory.ConnectorPunctuation
            || category == UnicodeCategory.Format;
    }

    public bool Equals(string? other)
    {
        if (string.IsNullOrEmpty(other))
        {
            return IsEmpty;
        }

        return string.Equals(_value, other, StringComparison.Ordinal);
    }

    public bool Equals(IdentifierString other) => string.Equals(_value, other._value, StringComparison.Ordinal);

    public override string ToString() => _value ?? "";

    public override bool Equals(object obj)
    {
        return obj switch
        {
            null => IsEmpty,
            IdentifierString identifier => Equals(identifier),
            string s => Equals(s),
            _ => false
        };
    }

    public override int GetHashCode() => _value?.GetHashCode() ?? 0;

    public static bool Equals(IdentifierString identifier, string? s) => identifier.Equals(s);

    public static bool Equals(IdentifierString identifier1, IdentifierString identifier2) => identifier1.Equals(identifier2);

    public static bool operator ==(IdentifierString identifier, string? s) => Equals(identifier, s);

    public static bool operator !=(IdentifierString identifier, string? s) => !Equals(identifier, s);

    public static bool operator ==(IdentifierString identifier1, IdentifierString identifier2) => 
        Equals(identifier1, identifier2);

    public static bool operator !=(IdentifierString identifier1, IdentifierString identifier2) => 
        !Equals(identifier1, identifier2);

    public static implicit operator string(IdentifierString identifier) => identifier.ToString();
}
