namespace Reqnroll.TestProjectGenerator.Dotnet
{
    public class VersionCommandBuilder : BaseCommandBuilder
    {
        public static VersionCommandBuilder Create(IOutputWriter outputWriter)
        {
            return new VersionCommandBuilder(outputWriter);
        }

        protected override string GetWorkingDirectory()
        {
            return ".";
        }

        protected override string BuildArguments()
        {
            return "--version";
        }

        public VersionCommandBuilder(IOutputWriter outputWriter) : base(outputWriter)
        {
        }
    }
}
