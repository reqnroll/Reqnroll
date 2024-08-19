using System;
using System.IO;

namespace Reqnroll.Configuration
{
    public class ReqnrollJsonLocator : IReqnrollJsonLocator
    {
        public const string JsonConfigurationFileName = "reqnroll.json";
        public const string CompatibilityJsonConfigurationFileName = "specflow.json";

        public string GetReqnrollJsonFilePath()
        {
            return GetJsonFilePath(JsonConfigurationFileName) ?? 
                   GetJsonFilePath(CompatibilityJsonConfigurationFileName);
        }

        private string GetJsonFilePath(string configurationFileName)
        {
            var reqnrollJsonFileInAppDomainBaseDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, configurationFileName);

            if (File.Exists(reqnrollJsonFileInAppDomainBaseDirectory))
            {
                return reqnrollJsonFileInAppDomainBaseDirectory;
            }

            var reqnrollJsonFileInCurrentDirectory = Path.Combine(Environment.CurrentDirectory, configurationFileName);

            if (File.Exists(reqnrollJsonFileInCurrentDirectory))
            {
                return reqnrollJsonFileInCurrentDirectory;
            }

            return null;
        }
    }
}
