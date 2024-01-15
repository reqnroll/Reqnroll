using System.IO;

namespace Reqnroll.TestProjectGenerator.Dotnet
{
    public partial class AddCommandBuilder
    {
        public class AddPackageCommandBuilder : BaseCommandBuilder
        {
            private string _projectFilePath;
            private string _packageName;
            private string _packageVersion;
            private bool _noRestore = false;

            public AddPackageCommandBuilder(IOutputWriter outputWriter) : base(outputWriter)
            {
            }


            public AddPackageCommandBuilder ToProject(string projectFilePath)
            {
                _projectFilePath = projectFilePath;
                return this;
            }

            public AddPackageCommandBuilder WithPackageName(string packageName)
            {
                _packageName = packageName;
                return this;
            }

            public AddPackageCommandBuilder WithPackageVersion(string packageVersion)
            {
                _packageVersion = packageVersion;
                return this;
            }

            protected override string GetWorkingDirectory()
            {
                return Path.GetDirectoryName(_projectFilePath);
            }

            protected override string BuildArguments()
            {
                string arguments = $"add {_projectFilePath} package {_packageName}";
                arguments = AddArgument(arguments, "-v", _packageVersion);

                if (_noRestore)
                {
                    arguments += " -n ";
                }

                return arguments;
            }

            public AddPackageCommandBuilder WithNoRestore()
            {
                _noRestore = true;
                return this;
            }
        }
    }
}
