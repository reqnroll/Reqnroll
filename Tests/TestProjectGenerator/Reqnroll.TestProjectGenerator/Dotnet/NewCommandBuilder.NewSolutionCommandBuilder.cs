namespace Reqnroll.TestProjectGenerator.Dotnet
{
    public partial class NewCommandBuilder
    {
        public class NewSolutionCommandBuilder : BaseCommandBuilder
        {
            protected string _name;
            protected string _rootPath;

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
                string arguments = $"new sln";
                arguments = AddArgument(arguments, "-n", _name);
                arguments = AddArgument(arguments, "-o", "\"" + _rootPath + "\"");

                return arguments;
            }


            public NewSolutionCommandBuilder(IOutputWriter outputWriter) : base(outputWriter)
            {
            }
        }
    }

}
