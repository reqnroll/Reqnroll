using System.IO;
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

        internal static string NormalizeDirectorySeparators(string path)
        {
            if (path == null)
                return null;

            switch (Path.DirectorySeparatorChar)
            {
                case '\\':
                    return path.Replace('/', '\\');
                case '/':
                    return path.Replace('\\', '/');
            }
            return path;
        }        

        [Given("there is a content file '(.*)' in the project as")]
        public void GivenThereIsAContentFileInTheProjectAs(string fileName, string fileContent)
        {
            fileName = NormalizeDirectorySeparators(fileName);
            _projectsDriver.AddFile(fileName, fileContent, "Content");
        }
    }
}
