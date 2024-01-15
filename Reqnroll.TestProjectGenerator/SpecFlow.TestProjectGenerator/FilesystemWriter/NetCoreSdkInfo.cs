namespace TechTalk.SpecFlow.TestProjectGenerator.FilesystemWriter
{
    public class NetCoreSdkInfo
    {
        internal NetCoreSdkInfo(string version)
        {
            Version = version;
        }

        public string Version { get; }
    }
}
