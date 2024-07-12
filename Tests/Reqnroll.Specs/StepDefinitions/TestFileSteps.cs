using System.IO;
using FluentAssertions;
using Reqnroll.Specs.Drivers.Parser;
using Reqnroll.Specs.Support;
using Reqnroll.TestProjectGenerator;
using Reqnroll.TestProjectGenerator.Driver;

namespace Reqnroll.Specs.StepDefinitions
{
    [Binding]
    public class TestFileSteps
    {
        private readonly TestFileManager _testFileManager;
        private readonly ParserDriver _parserDriver;
        private readonly ProjectsDriver _projectsDriver;
        private readonly ConfigurationDriver _configurationDriver;

        public TestFileSteps(TestFileManager testFileManager, ParserDriver parserDriver, ProjectsDriver projectsDriver, ConfigurationDriver configurationDriver)
        {
            _testFileManager = testFileManager;
            _parserDriver = parserDriver;
            _projectsDriver = projectsDriver;
            _configurationDriver = configurationDriver;
        }

        [When(@"the test file '(.*)' is parsed")]
        public void WhenTheTestFileIsParsed(string testFile)
        {
            string testFileContent = _testFileManager.GetTestFileContent(testFile);
            _parserDriver.FileContent = testFileContent;
            _parserDriver.ParseFile();
        }

        [When(@"the parsed result is saved to '(.*)'")]
        public void WhenTheParsedResultIsSavedTo(string parsedFileName)
        {
            var assemblyFolder = AssemblyFolderHelper.GetAssemblyFolder();

            _configurationDriver.PipelineMode.Should().BeFalse("parsed file saving can only be done from a development environment");
            _parserDriver.SaveSerializedFeatureTo(Path.Combine(assemblyFolder, @"..\..\..\TestFiles", parsedFileName));
        }

        [Then(@"the parsed result is the same as '(.*)'")]
        public void ThenTheParsedResultIsTheSameAs(string parsedFileName)
        {
            string expected = _testFileManager.GetTestFileContent(parsedFileName);
            _parserDriver.AssertParsedFeatureEqualTo(expected);
        }

        [Given(@"all test files are inluded in the project")]
        public void GivenAllTestFilesAreInludedInTheProject()
        {
            foreach (var testFile in _testFileManager.GetTestFeatureFiles())
            {
                string testFileContent = _testFileManager.GetTestFileContent(testFile);

                _projectsDriver.AddFeatureFile(testFileContent);
            }
        }

    }
}
