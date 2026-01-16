using Reqnroll.TestProjectGenerator.FilesystemWriter;

namespace Reqnroll.TestProjectGenerator.Dotnet;

public partial class NewCommandBuilder
{
    public class NewSolutionCommandBuilder(IOutputWriter outputWriter, DotNetSdkInfo sdk) : BaseCommandBuilder(outputWriter)
    {
        protected string _name;
        protected string _rootPath;
        protected DotNetSdkInfo _sdk = sdk;

        public NewSolutionCommandBuilder InFolder(string rootPath)
        {
            _rootPath = rootPath;
            return this;
        }

        public NewSolutionCommandBuilder WithName(string name)
        {
            _name = name;
            return this;
        }

        protected override string GetWorkingDirectory()
        {
            return _rootPath;
        }

        protected override string BuildArguments()
        {
            string arguments = "new sln";
            arguments = AddArgument(arguments, "-n", _name);
            arguments = AddArgument(arguments, "-o", "\"" + _rootPath + "\"");
            if (_sdk.GetParsedVersion().Major >= 10)
                arguments = AddArgument(arguments, "-f", "sln"); // force sln format (the default has changed to slnx)
            return arguments;
        }
    }
}