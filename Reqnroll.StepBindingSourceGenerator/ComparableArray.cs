using System.Collections;
using System.Collections.Immutable;

namespace Reqnroll.StepBindingSourceGenerator;

internal static class ComparableArray
{
    public static ComparableArray<T> CreateRange<T>(IEnumerable<T> items) => new(ImmutableArray.CreateRange(items));

    public static ComparableArray<T> Create<T>(T item) => new(ImmutableArray.Create(item));

    public static ComparableArray<T> CreateRange<T>(ReadOnlySpan<T> items)
    {
        var builder = ImmutableArray.CreateBuilder<T>(items.Length);

        foreach (var item in items)
        {
            builder.Add(item);
        }

        return new ComparableArray<T>(builder.ToImmutable());
    }
}

internal readonly struct ComparableArray<T>(ImmutableArray<T> values) : IEquatable<ComparableArray<T>>, IEnumerable<T>
{
    private readonly ImmutableArray<T> _values = values;

    public static ComparableArray<T> Empty { get; } = new ComparableArray<T>(ImmutableArray<T>.Empty);

    public int Length => _values.Length;

    public bool IsDefaultOrEmpty => _values.IsDefaultOrEmpty;

    public override bool Equals(object obj)
    {
        if (obj is ComparableArray<T> other)
        {
            return Equals(other);
        }

        return false;
    }

    public bool Equals(ComparableArray<T> other)
    {
        if (_values.IsDefaultOrEmpty)
        {
            return other._values.IsDefaultOrEmpty;
        }

        if (other._values.IsDefaultOrEmpty)
        {
            return false;
        }

        // This comparison checks if the two immutable arrays point to the same underlying array object.
        if (_values == other._values)
        {
            return true;
        }

        if (_values.Length != other._values.Length)
        {
            return false;
        }

        for (var i = 0; i < _values.Length; i++)
        {
            if (!Equals(_values[i], other._values[i]))
            {
                return false;
            }
        }

        return true;
    }

    public override int GetHashCode()
    {
        if (_values.IsDefaultOrEmpty)
        {
            return 0;
        }

        return _values.GetSequenceHashCode();
    }

    public ImmutableArray<T>.Enumerator GetEnumerator() => 
        (_values.IsDefault ? ImmutableArray<T>.Empty : _values).GetEnumerator();

    IEnumerator<T> IEnumerable<T>.GetEnumerator() => 
        ((IEnumerable<T>)(_values.IsDefault ? ImmutableArray<T>.Empty : _values)).GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => 
        ((IEnumerable<T>)(_values.IsDefault ? ImmutableArray<T>.Empty : _values)).GetEnumerator();

    public static implicit operator ComparableArray<T>(ImmutableArray<T> array) => new(array);
}
