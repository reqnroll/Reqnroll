namespace TechTalk.SpecFlow.TestProjectGenerator.Dotnet
{
    public partial class NewCommandBuilder
    {
        private readonly IOutputWriter _outputWriter;

        public NewCommandBuilder(IOutputWriter outputWriter)
        {
            _outputWriter = outputWriter;
        }

        internal static NewCommandBuilder Create(IOutputWriter outputWriter) => new NewCommandBuilder(outputWriter);

        public NewSolutionCommandBuilder Solution() => new NewSolutionCommandBuilder(_outputWriter);

        public NewProjectCommandBuilder Project() => new NewProjectCommandBuilder(_outputWriter);
    }
}
