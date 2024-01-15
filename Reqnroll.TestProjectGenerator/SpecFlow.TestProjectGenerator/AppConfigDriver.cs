using System.Configuration;

namespace TechTalk.SpecFlow.TestProjectGenerator
{
    public class AppConfigDriver
    {
        public string TestProjectFolderName => ConfigurationManager.AppSettings["testProjectFolder"] ?? "SF";
        public string VSTestPath => ConfigurationManager.AppSettings["vstestPath"] ?? "Common7\\IDE\\CommonExtensions\\Microsoft\\TestWindow";
    }
}