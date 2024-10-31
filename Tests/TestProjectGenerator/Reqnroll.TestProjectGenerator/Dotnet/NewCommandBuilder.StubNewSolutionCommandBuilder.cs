using Reqnroll.TestProjectGenerator.FilesystemWriter;

namespace Reqnroll.TestProjectGenerator.Dotnet;

public partial class NewCommandBuilder
{
    public class StubNewSolutionCommandBuilder : NewSolutionCommandBuilder
    {
        private readonly NetCoreSdkInfo _sdk;

        public StubNewSolutionCommandBuilder(IOutputWriter outputWriter, NetCoreSdkInfo sdk) : base(outputWriter)
        {
            _sdk = sdk;
        }

        public override CommandBuilder Build()
        {
            return new CacheAndCopyCommandBuilder(_outputWriter, _sdk, base.Build(), _rootPath, _name);

            //return new CopyFolderCommandBuilder(_outputWriter, @"C:\Temp\DotNetNewIssue\DotNetNewTest\DefaultTestProject", _folder);
            //return base.Build();
        }
    }
}