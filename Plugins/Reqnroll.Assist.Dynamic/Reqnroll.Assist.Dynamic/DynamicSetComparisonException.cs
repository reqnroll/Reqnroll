namespace Reqnroll.Assist.Dynamic;

public class DynamicSetComparisonException : Exception
{
    public IList<string> Differences { get; private set; }

    public DynamicSetComparisonException(string message) : base(message) { }

    public DynamicSetComparisonException(string message, IList<string> differences) : base(message)
    {
        Differences = differences;
    }
}