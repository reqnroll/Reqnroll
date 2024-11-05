using Reqnroll.TestProjectGenerator.FilesystemWriter;

namespace Reqnroll.TestProjectGenerator.Dotnet;

public partial class NewCommandBuilder
{
    public class StubNewProjectCommandBuilder(IOutputWriter outputWriter, NetCoreSdkInfo _sdk) : NewProjectCommandBuilder(outputWriter)
    {
        public override CommandBuilder Build()
        {
            return new CacheAndCopyCommandBuilder(_outputWriter, _sdk, base.Build(), _folder);
        }
    }
}