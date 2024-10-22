namespace Reqnroll.TestProjectGenerator.Dotnet;

public partial class NewCommandBuilder
{
    public class StubNewSolutionCommandBuilder : NewSolutionCommandBuilder
    {
        public StubNewSolutionCommandBuilder(IOutputWriter outputWriter) : base(outputWriter)
        {
        }

        public override CommandBuilder Build()
        {
            return new CacheAndCopyCommandBuilder(_outputWriter, base.Build(), _rootPath, _name);

            //return new CopyFolderCommandBuilder(_outputWriter, @"C:\Temp\DotNetNewIssue\DotNetNewTest\DefaultTestProject", _folder);
            //return base.Build();
        }
    }
}