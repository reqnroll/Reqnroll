using System;
using System.IO;
using Reqnroll.Specs.Drivers;
using Reqnroll.TestProjectGenerator;
using Reqnroll.TestProjectGenerator.Helpers;
using Reqnroll.UnitTestProvider;

namespace Reqnroll.Specs.Support
{
    [Binding]
    public class Hooks
    {
        private readonly ScenarioContext _scenarioContext;
        private readonly CurrentVersionDriver _currentVersionDriver;
        private readonly RuntimeInformationProvider _runtimeInformationProvider;
        private readonly IUnitTestRuntimeProvider _unitTestRuntimeProvider;
        private readonly TestProjectFolders _testProjectFolders;

        public Hooks(ScenarioContext scenarioContext, CurrentVersionDriver currentVersionDriver, RuntimeInformationProvider runtimeInformationProvider, IUnitTestRuntimeProvider unitTestRuntimeProvider, TestProjectFolders testProjectFolders)
        {
            _scenarioContext = scenarioContext;
            _currentVersionDriver = currentVersionDriver;
            _runtimeInformationProvider = runtimeInformationProvider;
            _unitTestRuntimeProvider = unitTestRuntimeProvider;
            _testProjectFolders = testProjectFolders;
        }

        [BeforeScenario]
        public void BeforeScenario()
        {
            _currentVersionDriver.NuGetVersion = NuGetPackageVersion.Version;
            _currentVersionDriver.ReqnrollNuGetVersion = NuGetPackageVersion.Version;
            _scenarioContext.ScenarioContainer.RegisterTypeAs<OutputConnector, IOutputWriter>();
        }

        [AfterScenario]
        public void AfterScenario()
        {
            if (_scenarioContext.TestError == null && _testProjectFolders.IsPathToSolutionFileSet)
            {
                try
                {
                    FileSystemHelper.DeleteFolder(_testProjectFolders.PathToSolutionDirectory);
                }
                catch (Exception)
                {
                    // ignored
                }
            }
        }

        [BeforeTestRun]
        public static void BeforeTestRun()
        {
            var appConfigDriver = new AppConfigDriver();
            var folders = new Folders(appConfigDriver);

            DeletePackageVersionFolders();
            DeleteOldTestRunData(folders);
        }

        private static void DeleteOldTestRunData(Folders folders)
        {
            try
            {
                FileSystemHelper.DeleteFolderContent(folders.FolderToSaveGeneratedSolutions);
            }
            catch (Exception)
            {
                // ignored
            }
        }

        private static void DeletePackageVersionFolders()
        {
            if (!HaveRightsToDeletePackages()) return;

            var currentVersionDriver = new CurrentVersionDriver {NuGetVersion = NuGetPackageVersion.Version };

            string[] packageNames = { "Reqnroll", "Reqnroll.CustomPlugin", "Reqnroll.MsTest", "Reqnroll.NUnit", "Reqnroll.NUnit.Runners", "Reqnroll.Tools.MsBuild.Generation", "Reqnroll.xUnit" };
            
            foreach (var name in packageNames)
            {
                string hooksPath = Path.Combine(Environment.ExpandEnvironmentVariables("%USERPROFILE%"), ".nuget", "packages", name, currentVersionDriver.NuGetVersion);
                FileSystemHelper.DeleteFolder(hooksPath);
            }
        }

        private static bool HaveRightsToDeletePackages()
        {
            var azureAgentOs = Environment.GetEnvironmentVariable("AGENT_OS");
            return azureAgentOs != "Windows_NT";
        }
    }
}
