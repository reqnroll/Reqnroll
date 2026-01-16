using System;

namespace Reqnroll.TestProjectGenerator.FilesystemWriter
{
    public class DotNetSdkInfo
    {
        internal DotNetSdkInfo(string version)
        {
            Version = version;
        }

        public string Version { get; }

        public Version GetParsedVersion()
        {
            return Version != null ? new Version(Version) : throw new InvalidOperationException("Version is null");
        }
    }
}
