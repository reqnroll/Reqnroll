namespace Reqnroll.TestProjectGenerator.Dotnet
{
    public partial class SolutionCommandBuilder

    {
        private readonly IOutputWriter _outputWriter;

        public SolutionCommandBuilder(IOutputWriter outputWriter)
        {
            _outputWriter = outputWriter;
        }

        public static SolutionCommandBuilder Create(IOutputWriter outputWriter) => new SolutionCommandBuilder(outputWriter);

        public AddProjectSolutionCommandBuilder AddProject() => new StubAddProjectSolutionCommandBuilder(_outputWriter);
    }
}
