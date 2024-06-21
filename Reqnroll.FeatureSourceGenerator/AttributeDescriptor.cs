using System.Collections;
using System.Collections.Immutable;
using System.ComponentModel;

namespace Reqnroll.FeatureSourceGenerator;

/// <summary>
/// Provides a description of a .NET attribute instance.
/// </summary>
/// <param name="type">The attribute's type.</param>
/// <param name="positionalArguments">The positional arguments of the attribute.</param>
/// <param name="namedArguments">The named arguments of the attribute.</param>
public class AttributeDescriptor(
    NamedTypeIdentifier type,
    ImmutableArray<object?>? positionalArguments = null,
    ImmutableDictionary<string, object?>? namedArguments = null)
    : IEquatable<AttributeDescriptor>
{
    public NamedTypeIdentifier Type { get; } = type;

    public ImmutableArray<object?> PositionalArguments { get; } = 
        ThrowIfArgumentTypesNotValid(
            positionalArguments.GetValueOrDefault(ImmutableArray<object?>.Empty),
            nameof(positionalArguments));

    public ImmutableDictionary<string, object?> NamedArguments { get; } =
        ThrowIfArgumentTypesNotValid(namedArguments ?? ImmutableDictionary<string, object?>.Empty, nameof(namedArguments));

    private int? _hashCode;

    private static ImmutableArray<object?> ThrowIfArgumentTypesNotValid(
        ImmutableArray<object?> positionalArguments,
        string paramName)
    {
        foreach (var item in positionalArguments)
        {
            ThrowIfArgumentTypeNotValid(item, paramName);
        }

        return positionalArguments;
    }

    private static void ThrowIfArgumentTypeNotValid(object? value, string paramName)
    {
        switch (value)
        {
            case null:
            case bool _:
            case byte _:
            case char _:
            case double _:
            case float _:
            case int _:
            case long _:
            case sbyte _:
            case short _:
            case string _:
            case uint _:
            case ulong _:
            case ushort _:
            case Type _:
            case Enum _:
                break;

            case Array _:
                throw new ArgumentException(
                    "Mutable arrays may not be used with AttributeDescriptor. To represent an array use ImmutableArray<T> instead.",
                    paramName);

            default:
                var valueType = value.GetType();
                if (valueType == typeof(object))
                {
                    break;
                }

                if (IsImmutableArrayType(valueType))
                {
                    var array = (IEnumerable)value;
                    foreach(var item in array)
                    {
                        ThrowIfArgumentTypeNotValid(item, paramName);
                    }

                    break;
                }

                throw new ArgumentException(
                    $"Instances of type {value.GetType()} cannot be used as attribute arguments.",
                    paramName);
        }
    }

    private static ImmutableDictionary<string, object?> ThrowIfArgumentTypesNotValid(
        ImmutableDictionary<string, object?> namedArguments,
        string paramName)
    {
        foreach (var item in namedArguments.Values)
        {
            ThrowIfArgumentTypeNotValid(item, paramName);
        }

        return namedArguments;
    }

    public override int GetHashCode()
    {
        _hashCode ??= CalculateHashCode();

        return _hashCode.Value;
    }

    private int CalculateHashCode()
    { 
        unchecked
        {
            var hash = 47;

            hash *= 13 + Type.GetHashCode();

            var index = 0;
            foreach (var item in PositionalArguments)
            {
                hash *= 13 + index++ + GetItemHash(item);
            }

            foreach(var (name, value) in NamedArguments)
            {
                hash *= 13 + name.GetHashCode() + GetItemHash(value);
            }

            return hash;
        }
    }

    private int GetItemHash(object? item)
    {
        if (item is null)
        {
            return 0;
        }

        if (IsImmutableArray(item))
        {
            return GetSequenceHash((IEnumerable)item);
        }

        return item.GetHashCode();
    }

    private int GetSequenceHash(IEnumerable sequence)
    {
        unchecked
        {
            var hash = 127;
            var index = 0;

            foreach (var item in sequence)
            {
                hash *= 13 + index++ + GetItemHash(item);
            }

            return hash;
        }
    }

    private static bool IsImmutableArray(object value) => IsImmutableArrayType(value.GetType());

    private static bool IsImmutableArrayType(Type type) =>
        type.IsGenericType && type.GetGenericTypeDefinition() == typeof(ImmutableArray<>);

    public override bool Equals(object obj) => Equals(obj as AttributeDescriptor);

    public virtual bool Equals(AttributeDescriptor? other)
    {
        if (other is null)
        {
            return false;
        }

        if (ReferenceEquals(other, this))
        {
            return true;
        }

        if (GetHashCode() != other.GetHashCode())
        {
            return false;
        }

        return Type.Equals(other.Type) &&
            ArgumentsEqual(other);
    }

    private bool ArgumentsEqual(AttributeDescriptor other)
    {
        return ArgumentSequenceEqual(PositionalArguments, other.PositionalArguments) &&
            ArgumentDictionaryEqual(NamedArguments, other.NamedArguments);
    }

    private static bool ArgumentDictionaryEqual(
        ImmutableDictionary<string, object?> first,
        ImmutableDictionary<string, object?> second)
    {
        if (ReferenceEquals(first, second))
        {
            return true;
        }

        if (first.Count != second.Count)
        {
            return false;
        }

        foreach (var (name, firstArgument) in first)
        {
            if (!second.TryGetValue(name, out var secondArgument))
            {
                return false;
            }

            if (!ArgumentEquals(firstArgument, secondArgument))
            {
                return false;
            }
        }

        return true;
    }

    private static bool ArgumentEquals(object? first, object? second)
    {
        if (ReferenceEquals(first, second))
        {
            return true;
        }

        if (first is null || second is null)
        {
            return false;
        }

        var firstType = first.GetType();
        if (IsImmutableArrayType(firstType))
        {
            if (second.GetType() != firstType)
            {
                return false;
            }

            return ArgumentSequenceEqual((IList)first, (IList)second);
        }

        return first.Equals(second);
    }

    private static bool ArgumentSequenceEqual(IList first, IList second)
    {
        if (first.Count != second.Count)
        {
            return false;
        }

        for (int i = 0; i < first.Count; i++)
        {
            if (!ArgumentEquals(first[i], second[i]))
            {
                return false;
            }
        }

        return true;
    }

    private static bool ArgumentSequenceEqual(ImmutableArray<object?> first, ImmutableArray<object?> second)
    {
        if (first.Length != second.Length)
        {
            return false;
        }

        for (int i = 0; i < first.Length; i++)
        {
            if (!ArgumentEquals(first[i], second[i]))
            {
                return false;
            }
        }

        return true;
    }

    public override string ToString()
    {
        var positional = PositionalArguments.Length > 0 ? string.Join(", ", PositionalArguments.Select(ToLiteralString)) : null;
        var named = NamedArguments.Count > 0 ?
            string.Join(", ", NamedArguments.Select(kvp => $"{kvp.Key}={ToLiteralString(kvp.Value)}")) :
            null;

        string argumentList;

        if (positional != null && named == null)
        {
            argumentList = $"({positional})";
        }
        else if (positional == null && named != null)
        {
            argumentList = $"({named})";
        }
        else if (positional != null && named != null)
        {
            argumentList = $"({positional}, {named})";
        }
        else
        {
            argumentList = string.Empty;
        }

        return $"[{Type}{argumentList}]";
    }

    private static string ToLiteralString(object? value)
    {
        if (value is null)
        {
            return "null";
        }

        if (value is string s)
        {
            return $"\"{s}\"";
        }

        var type = value.GetType();
        if (IsImmutableArrayType(type))
        {
            var itemType = type.GetGenericArguments()[0];
            var values = ((IEnumerable)value).Cast<object?>().Select(ToLiteralString);

            return $"new {GetTypeIdentifier(itemType)}[] {{{string.Join(", ", values)}}}";
        }

        return value.ToString();
    }

    private static string GetTypeIdentifier(Type type)
    {
        if (CSharp.CSharpSyntax.TypeAliases.TryGetValue(type, out var alias))
        {
            return alias;
        }

        return type.FullName;
    }

    public AttributeDescriptor WithPositionalArguments(params object?[] positionalArguments)
    {
        return new AttributeDescriptor(Type, positionalArguments.ToImmutableArray(), NamedArguments);
    }

    public AttributeDescriptor WithNamedArguments(object namedArguments)
    {
        var arguments = new Dictionary<string, object?>(StringComparer.OrdinalIgnoreCase);

        foreach (PropertyDescriptor propertyDescriptor in TypeDescriptor.GetProperties(namedArguments))
        {
            arguments.Add(propertyDescriptor.Name, propertyDescriptor.GetValue(namedArguments));
        }

        return new AttributeDescriptor(Type, PositionalArguments, arguments.ToImmutableDictionary());
    }
}
