using System.IO;

namespace Reqnroll.TestProjectGenerator.Dotnet
{
    public partial class NewCommandBuilder
    {
        public class StubNewProjectCommandBuilder : NewProjectCommandBuilder
        {
            public StubNewProjectCommandBuilder(IOutputWriter outputWriter) : base(outputWriter)
            {
            }

            public override CommandBuilder Build()
            {
                return new CacheAndCopyCommandBuilder(_outputWriter, base.Build(), _folder);

                //return new CopyFolderCommandBuilder(_outputWriter, @"C:\Temp\DotNetNewIssue\DotNetNewTest\DefaultTestProject", _folder);
                //return base.Build();
            }
        }
    }
}
