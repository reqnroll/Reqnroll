using Reqnroll.TestProjectGenerator.Driver;

namespace Reqnroll.Specs.StepDefinitions
{
    [Binding]
    public class ProjectSteps
    {
        private readonly ProjectsDriver _projectsDriver;
        private readonly CompilationDriver _compilationDriver;
        private readonly CompilationResultDriver _compilationResultDriver;

        public ProjectSteps(ProjectsDriver projectsDriver, CompilationDriver compilationDriver, CompilationResultDriver compilationResultDriver)
        {
            _projectsDriver = projectsDriver;
            _compilationDriver = compilationDriver;
            _compilationResultDriver = compilationResultDriver;
        }

        [Given(@"there is a Reqnroll project")]
        public void GivenThereIsAReqnrollProject()
        {
            _projectsDriver.CreateReqnrollProject("C#");
        }

        [Given(@"it is using Reqnroll\.Tools\.MSBuild\.Generator")]
        public void GivenItIsUsingReqnroll_Tools_MSBuild_Generator()
        {
        }


        [Given(@"parallel execution is enabled")]
        public void GivenParallelExecutionIsEnabled()
        {
            _projectsDriver.EnableTestParallelExecution();
        }


        [Given(@"I have a '(.*)' test project")]
        public void GivenIHaveATestProject(string language)
        {
            _projectsDriver.CreateProject(language);
        }

        [Given(@"fluent assertion nuget package is added")]
        public void GivenFluentAssertion()
        {
            _projectsDriver.AddNuGetPackage("FluentAssertions", "5.3.0");
        }

        [When(@"I compile the solution")]
        public void WhenTheProjectIsCompiled()
        {
            _compilationDriver.CompileSolution(failOnError: false);
        }

        [Then(@"no compilation errors are reported")]
        public void ThenNoCompilationErrorsAreReported()
        {
            _compilationResultDriver.CheckSolutionShouldHaveCompiled();
        }

        [Then(@"is a compilation error")]
        public void ThenIsACompilationError()
        {
            _compilationResultDriver.CheckSolutionShouldHaveCompileError();
        }
    }
}
