using Reqnroll.Utils;
using Reqnroll.TestProjectGenerator.Driver;

namespace Reqnroll.Specs.StepDefinitions
{
    [Binding]
    public sealed class ContentFileSteps
    {
        private readonly ProjectsDriver _projectsDriver;

        public ContentFileSteps(ProjectsDriver projectsDriver)
        {
            _projectsDriver = projectsDriver;
        }

        [Given("there is a content file '(.*)' in the project as")]
        public void GivenThereIsAContentFileInTheProjectAs(string fileName, string fileContent)
        {
            fileName = FileSystemHelper.NormalizeDirectorySeparators(fileName);
            _projectsDriver.AddFile(fileName, fileContent, "Content");
        }
    }
}
