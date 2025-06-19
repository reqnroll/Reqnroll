namespace Reqnroll.Formatters.ExecutionTracking;

public class TestStepArgument
{
    public string Value;
    public int? StartOffset;
    public string Type;

    public TestStepArgument()
    {
    }

    public TestStepArgument(string value, int? startOffset, string type)
    {
        Value = value;
        StartOffset = startOffset;
        Type = type;
    }
}
