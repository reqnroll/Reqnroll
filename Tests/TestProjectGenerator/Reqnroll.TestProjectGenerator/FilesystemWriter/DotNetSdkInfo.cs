namespace Reqnroll.TestProjectGenerator.FilesystemWriter
{
    public class DotNetSdkInfo
    {
        internal DotNetSdkInfo(string version)
        {
            Version = version;
        }

        public string Version { get; }
    }
}
