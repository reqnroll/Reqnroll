using System.Collections.Immutable;

namespace Reqnroll.FeatureSourceGenerator.SourceModel;

public class DataTable(ImmutableArray<string> headings, ImmutableArray<ImmutableArray<string>> rows) : IEquatable<DataTable?>
{
    public ImmutableArray<string> Headings { get; } = headings.IsDefault ? ImmutableArray<string>.Empty : headings;

    public ImmutableArray<ImmutableArray<string>> Rows { get; } = rows.IsDefault ? ImmutableArray<ImmutableArray<string>>.Empty : rows;

    public bool Equals(DataTable? other)
    {
        if (other is null)
        {
            return false;
        }

        if (ReferenceEquals(this, other))
        {
            return true;
        }

        if (!Headings.SequenceEqual(other.Headings) || Rows.Length != other.Rows.Length)
        {
            return false;
        }

        for (var i = 0; i < Rows.Length; i++)
        {
            var row = Rows[i];
            var otherRow = other.Rows[i];

            if (!row.SequenceEqual(otherRow))
            {
                return false;
            }
        }

        return true;
    }

    public override bool Equals(object obj) => Equals(obj as DataTable);

    public override int GetHashCode()
    {
        unchecked
        {
            var hash = 25979651;

            hash *= 33473171 + Rows.GetSequenceHashCode();
            
            foreach (var row in Rows)
            {
                hash *= 33473171 + row.GetSequenceHashCode();
            }

            return hash;
        }
    }
}
