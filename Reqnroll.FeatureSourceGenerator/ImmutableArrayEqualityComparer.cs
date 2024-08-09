using System.Collections.Immutable;

namespace Reqnroll.FeatureSourceGenerator;

public class ImmutableArrayEqualityComparer<T>(IEqualityComparer<T> itemComparer) : IEqualityComparer<ImmutableArray<T>>
{
    public ImmutableArrayEqualityComparer() : this(EqualityComparer<T>.Default)
    {
    }

    public IEqualityComparer<T> ItemComparer { get; } = itemComparer;

    public static ImmutableArrayEqualityComparer<T> Default { get; } = new ImmutableArrayEqualityComparer<T>();

    public bool Equals(ImmutableArray<T> x, ImmutableArray<T> y)
    {
        if (x.Equals(y))
        {
            return true;
        }

        return x.SequenceEqual(y, ItemComparer);
    }

    public int GetHashCode(ImmutableArray<T> obj) => obj.GetSequenceHashCode(ItemComparer);
}
