using Reqnroll;

// ReSharper disable once CheckNamespace
namespace TechTalk.SpecFlow;

// ReSharper disable once InconsistentNaming
public class ISpecFlowOutputHelper(IReqnrollOutputHelper originalHelper) : IReqnrollOutputHelper
{
    public void WriteLine(string message)
    {
        originalHelper.WriteLine(message);
    }

    public void WriteLine(string format, params object[] args)
    {
        originalHelper.WriteLine(format, args);
    }

    public void AddAttachment(string filePath)
    {
        originalHelper.AddAttachment(filePath);
    }
}