namespace Reqnroll.Assist.Dynamic;

public class DynamicInstanceComparisonException : Exception
{
    public IList<string> Differences { get; private set; }

    public DynamicInstanceComparisonException(IList<string> diffs)
        : base("There were some difference between the table and the instance")
    {
        Differences = diffs;
    }
}