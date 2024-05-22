namespace Reqnroll.TestProjectGenerator.Dotnet
{
    public class BuildCommandBuilder : BaseCommandBuilder
    {
        internal static BuildCommandBuilder Create(IOutputWriter outputWriter)
        {
            return new BuildCommandBuilder(outputWriter);
        }

        protected override string GetWorkingDirectory()
        {
            return ".";
        }

        protected override string BuildArguments()
        {
            return "build --nologo";
        }

        public BuildCommandBuilder(IOutputWriter outputWriter) : base(outputWriter)
        {
        }
    }

}
