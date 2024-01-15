namespace TechTalk.SpecFlow.TestProjectGenerator.Dotnet
{
    public class DotNet
    {
        public static NewCommandBuilder New(IOutputWriter outputWriter) => NewCommandBuilder.Create(outputWriter);
        public static BuildCommandBuilder Build(IOutputWriter outputWriter) => BuildCommandBuilder.Create(outputWriter);
        public static SolutionCommandBuilder Sln(IOutputWriter outputWriter) => SolutionCommandBuilder.Create(outputWriter);
        public static VersionCommandBuilder Version(IOutputWriter outputWriter) => VersionCommandBuilder.Create(outputWriter);
        public static AddCommandBuilder Add(IOutputWriter outputWriter) => AddCommandBuilder.Create(outputWriter);
    }
}
