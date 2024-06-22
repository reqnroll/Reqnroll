using FluentAssertions;

namespace Reqnroll.TestProjectGenerator.Driver
{
    public class CompilationResultDriver
    {
        public CompileResult CompileResult { get; set; }

        public void CheckSolutionShouldHaveCompiled()
        {
            CompileResult.Should().NotBeNull("the project should have compiled");
            CompileResult.IsSuccessful.Should().BeTrue("the project should have compiled successfully.\r\n\r\n------ Build output ------\r\n{0}", CompileResult.Output);
        }

        public void CheckSolutionShouldHaveCompileError()
        {
            CompileResult.Should().NotBeNull("the project should have compiled");
            CompileResult.IsSuccessful.Should().BeFalse("There should be a compile error");
        }

        public bool CheckCompileOutputForString(string str)
        {
            return CompileResult.Output.Contains(str);
        }

        public void CheckSolutionShouldUseAppConfig()
        {
            CompileResult.Output.Should().Contain("Using app.config");
        }

        public void CheckSolutionShouldUseReqnrollJson()
        {
            CompileResult.Output.Should().Contain("Using reqnroll.json");
        }
    }
}
