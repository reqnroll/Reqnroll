using System;
using System.IO;

namespace Reqnroll.Configuration
{
    public class ReqnrollJsonLocator : IReqnrollJsonLocator
    {
        public const string JsonConfigurationFileName = "reqnroll.json";

        public string GetReqnrollJsonFilePath()
        {
            var reqnrollJsonFileInAppDomainBaseDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, JsonConfigurationFileName);

            if (File.Exists(reqnrollJsonFileInAppDomainBaseDirectory))
            {
                return reqnrollJsonFileInAppDomainBaseDirectory;
            }

            var reqnrollJsonFileTwoDirectoriesUp = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "..", JsonConfigurationFileName);

            if (File.Exists(reqnrollJsonFileTwoDirectoriesUp))
            {
                return reqnrollJsonFileTwoDirectoriesUp;
            }

            var reqnrollJsonFileInCurrentDirectory = Path.Combine(Environment.CurrentDirectory, JsonConfigurationFileName);

            if (File.Exists(reqnrollJsonFileInCurrentDirectory))
            {
                return reqnrollJsonFileInCurrentDirectory;
            }

            return null;
        }
    }
}
