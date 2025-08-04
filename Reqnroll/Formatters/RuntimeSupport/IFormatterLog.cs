namespace Reqnroll.Formatters.RuntimeSupport;

public interface IFormatterLog
{
    public void WriteMessage(string message);
    public void DumpMessages();
}