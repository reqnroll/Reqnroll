using System.Collections.Immutable;

namespace Reqnroll.FeatureSourceGenerator.SourceModel;
public sealed class StepInvocation(
    StepType type,
    FileLinePositionSpan position,
    string keyword,
    string text,
    ImmutableArray<IdentifierString> arguments = default,
    DataTable? dataTableArgument = default,
    string? docStringArgument = default) : IEquatable<StepInvocation?>
{
    public StepType Type { get; } = type;

    public FileLinePositionSpan Position { get; } = position;

    public string Keyword { get; } = keyword;

    public string Text { get; } = text;

    public ImmutableArray<IdentifierString> Arguments { get; } = 
        arguments.IsDefault ? ImmutableArray<IdentifierString>.Empty : arguments;

    public DataTable? DataTableArgument { get; } = dataTableArgument;

    public string? DocStringArgument { get; } = docStringArgument;

    public bool Equals(StepInvocation? other)
    {
        if (other is null)
        {
            return false;
        }

        if (ReferenceEquals(this, other))
        {
            return true;
        }

        return Type.Equals(other.Type) &&
            Position.Equals(other.Position) &&
            Keyword.Equals(other.Keyword) &&
            Text.Equals(other.Text) &&
            (Arguments.Equals(other.Arguments) || Arguments.SequenceEqual(other.Arguments)) &&
            (DataTableArgument is null && other.DataTableArgument is null || 
                DataTableArgument is not null && DataTableArgument.Equals(other.DataTableArgument)) &&
            string.Equals(DocStringArgument, other.DocStringArgument);
    }

    public override int GetHashCode()
    {
        unchecked
        {
            var hash = 92321497;

            hash *= 47886541 + Type.GetHashCode();
            hash *= 47886541 + Position.GetHashCode();
            hash *= 47886541 + Keyword.GetHashCode();
            hash *= 47886541 + Text.GetHashCode();
            hash *= 47886541 + Arguments.GetSequenceHashCode();

            if (DataTableArgument != null)
            {
                hash *= 47886541 + DataTableArgument.GetHashCode();
            }

            if (DocStringArgument != null)
            {
                hash *= 47886541 + DocStringArgument.GetHashCode();
            }

            return hash;
        }
    }
}
