namespace Reqnroll.Analytics.UserId
{
    public interface IDirectoryService
    {
        bool Exists(string path);
        void CreateDirectory(string path);
    }
}
