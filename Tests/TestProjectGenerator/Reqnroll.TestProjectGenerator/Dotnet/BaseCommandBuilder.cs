namespace Reqnroll.TestProjectGenerator.Dotnet
{
    public abstract class BaseCommandBuilder
    {
        protected readonly IOutputWriter _outputWriter;
        private const string ExecutablePath = "dotnet";

        protected BaseCommandBuilder(IOutputWriter outputWriter)
        {
            _outputWriter = outputWriter;
        }

        public virtual CommandBuilder Build()
        {
            return new CommandBuilder(_outputWriter, ExecutablePath, BuildArguments(), GetWorkingDirectory());
        }

        protected abstract string GetWorkingDirectory();

        protected abstract string BuildArguments();

        protected string AddArgument(string argumentsFormat, string option, string value)
        {
            if (!string.IsNullOrWhiteSpace(value))
            {
                argumentsFormat += $" {option} {value}";
            }

            return argumentsFormat;
        }
    }
}
