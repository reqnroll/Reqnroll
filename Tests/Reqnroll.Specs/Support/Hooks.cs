using Reqnroll.TestProjectGenerator;
using Reqnroll.TestProjectGenerator.Data;
using Reqnroll.TestProjectGenerator.Helpers;

namespace Reqnroll.Specs.Support;

[Binding]
public class Hooks
{
    private readonly ScenarioContext _scenarioContext;
    private readonly CurrentVersionDriver _currentVersionDriver;
    private readonly TestProjectFolders _testProjectFolders;
    private FolderCleaner _folderCleaner;

    public Hooks(ScenarioContext scenarioContext, CurrentVersionDriver currentVersionDriver, TestProjectFolders testProjectFolders)
    {
        _scenarioContext = scenarioContext;
        _currentVersionDriver = currentVersionDriver;
        _testProjectFolders = testProjectFolders;
    }

    [BeforeScenario]
    public void BeforeScenario()
    {
        _scenarioContext.ScenarioContainer.RegisterInstanceAs(
            new TestRunConfiguration
            {
                ProgrammingLanguage = TestProjectGenerator.ProgrammingLanguage.CSharp, 
                ProjectFormat = ProjectFormat.New, 
                TargetFramework = TargetFramework.Net80, 
                UnitTestProvider = TestProjectGenerator.UnitTestProvider.MSTest, 
                ConfigurationFormat = ConfigurationFormat.Json
            });

        _currentVersionDriver.ReqnrollNuGetVersion = NuGetPackageVersion.Version;
        _scenarioContext.ScenarioContainer.RegisterTypeAs<OutputConnector, IOutputWriter>();

        _folderCleaner = _scenarioContext.ScenarioContainer.Resolve<FolderCleaner>();

        // deleting old project folders (the actual deletion runs only once per test run)
        _folderCleaner.EnsureOldRunFoldersCleaned();
    }

    [AfterScenario]
    public void AfterScenario()
    {
        if (_scenarioContext.TestError == null && _testProjectFolders.IsPathToSolutionFileSet)
        {
            // making sure that the temporary files are deleted if the scenario succeeded anyway
            _folderCleaner.CleanSolutionFolder();
        }
    }
}