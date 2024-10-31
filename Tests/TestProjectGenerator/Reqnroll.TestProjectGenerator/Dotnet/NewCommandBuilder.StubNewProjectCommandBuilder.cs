using Reqnroll.TestProjectGenerator.FilesystemWriter;
using System.IO;

namespace Reqnroll.TestProjectGenerator.Dotnet
{
    public partial class NewCommandBuilder
    {
        public class StubNewProjectCommandBuilder : NewProjectCommandBuilder
        {
            private readonly NetCoreSdkInfo _sdk;

            public StubNewProjectCommandBuilder(IOutputWriter outputWriter, NetCoreSdkInfo sdk) : base(outputWriter)
            {
                _sdk = sdk;
            }

            public override CommandBuilder Build()
            {
                return new CacheAndCopyCommandBuilder(_outputWriter, _sdk, base.Build(), _folder);

                //return new CopyFolderCommandBuilder(_outputWriter, @"C:\Temp\DotNetNewIssue\DotNetNewTest\DefaultTestProject", _folder);
                //return base.Build();
            }
        }
    }
}
