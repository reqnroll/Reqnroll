using System.Collections.Immutable;

namespace Reqnroll.FeatureSourceGenerator.SourceModel;
public class StepInvocation(
    StepType type,
    int sourceLineNumber,
    string keyword,
    string text,
    ImmutableArray<IdentifierString> arguments = default)
{
    public StepType Type { get; } = type;

    public int SourceLineNumber { get; } = sourceLineNumber;

    public string Keyword { get; } = keyword;

    public string Text { get; } = text;

    public ImmutableArray<IdentifierString> Arguments { get; } = 
        arguments.IsDefault ? ImmutableArray<IdentifierString>.Empty : arguments;


    public virtual bool Equals(StepInvocation? other)
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
            SourceLineNumber.Equals(other.SourceLineNumber) &&
            Keyword.Equals(other.Keyword) &&
            Text.Equals(other.Text) &&
            (Arguments.Equals(other.Arguments) || Arguments.SequenceEqual(other.Arguments));
    }

    public override int GetHashCode()
    {
        unchecked
        {
            var hash = 92321497;

            hash *= 47886541 + Type.GetHashCode();
            hash *= 47886541 + SourceLineNumber.GetHashCode();
            hash *= 47886541 + Keyword.GetHashCode();
            hash *= 47886541 + Text.GetHashCode();
            hash *= 47886541 + Arguments.GetSequenceHashCode();

            return hash;
        }
    }
}
