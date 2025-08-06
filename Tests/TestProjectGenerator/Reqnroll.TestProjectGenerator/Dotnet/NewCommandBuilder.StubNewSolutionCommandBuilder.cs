using Reqnroll.TestProjectGenerator.FilesystemWriter;

namespace Reqnroll.TestProjectGenerator.Dotnet;

public partial class NewCommandBuilder
{
    public class StubNewSolutionCommandBuilder(IOutputWriter outputWriter, DotNetSdkInfo _sdk) : NewSolutionCommandBuilder(outputWriter)
    {
        public override CommandBuilder Build()
        {
            return new CacheAndCopyCommandBuilder(_outputWriter, _sdk, base.Build(), _rootPath, _name);
        }
    }
}