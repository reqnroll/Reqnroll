using TechTalk.SpecFlow.TestProjectGenerator.FilesystemWriter;

namespace TechTalk.SpecFlow.TestProjectGenerator.Driver
{
    public class SolutionWriteToDiskDriver
    {
        private readonly SolutionDriver _solutionDriver;
        private readonly SolutionWriter _solutionWriter;
        private readonly TestProjectFolders _testProjectFolders;
        private readonly NuGetRestorerFactory _nugetRestorerFactory;
        private bool _isWrittenToDisk;

        public SolutionWriteToDiskDriver(SolutionDriver solutionDriver, SolutionWriter solutionWriter, TestProjectFolders testProjectFolders, NuGetRestorerFactory nugetRestorerFactory)
        {
            _solutionDriver = solutionDriver;
            _solutionWriter = solutionWriter;
            _testProjectFolders = testProjectFolders;
            _nugetRestorerFactory = nugetRestorerFactory;
        }

        public void WriteSolutionToDisk(bool? treatWarningsAsErrors = null)
        {
            if (_isWrittenToDisk)
            {
                return;
            }

            foreach (var project in _solutionDriver.Projects.Values)
            {
                project.IsTreatWarningsAsErrors = treatWarningsAsErrors;
                project.GenerateSpecFlowConfigurationFile();
            }

            var solution = _solutionDriver.GetSolution();

            _solutionWriter.WriteToFileSystem(solution, _testProjectFolders.PathToSolutionDirectory);

            foreach (var project in solution.Projects)
            {
                var nugetRestorerForProject = _nugetRestorerFactory.GetNuGetRestorerForProject(project);
                nugetRestorerForProject.RestoreForProject(project);
            }

            _isWrittenToDisk = true;
        }
    }
}
