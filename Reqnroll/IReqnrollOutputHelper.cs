namespace Reqnroll
{
    public interface IReqnrollOutputHelper
    {
        void WriteLine(string message);
        void WriteLine(string format, params object[] args);
        void AddAttachment(string filePath);
    }
}