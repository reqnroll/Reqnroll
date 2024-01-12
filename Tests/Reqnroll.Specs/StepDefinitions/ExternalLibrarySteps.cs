using Reqnroll.TestProjectGenerator.Driver;

namespace Reqnroll.Specs.StepDefinitions
{
    [Binding]
    public class ExternalLibrarySteps : Steps
    {
        private readonly SolutionDriver _solutionDriver;
        private readonly ProjectsDriver _projectsDriver;

        public ExternalLibrarySteps(SolutionDriver solutionDriver, ProjectsDriver projectsDriver)
        {
            _solutionDriver = solutionDriver;
            _projectsDriver = projectsDriver;
        }

        [Given(@"there is an external class library project '(.*)'")]
        public void GivenThereIsAnExternalClassLibraryProject(string libraryName)
        {
            GivenThereIsAnExternalClassLibraryProject("C#", libraryName);
        }

        [Given(@"there is an external (.+) class library project '(.*)'")]
        public void GivenThereIsAnExternalClassLibraryProject(string language, string libraryName)
        {
            _projectsDriver.CreateProject(libraryName, language);
            _solutionDriver.Projects[libraryName].IsReqnrollFeatureProject = false;
        }

        [Given(@"there is a reference between the Reqnroll project and the '(.*)' project")]
        public void GivenThereIsAReqnrollProjectWithAReferenceToTheExternalLibrary(string referencedProject)
        {
            _projectsDriver.AddProjectReference(referencedProject);
        }


    }
}
