namespace Reqnroll.TestProjectGenerator.Dotnet
{
    public class CommandResult
    {
        public CommandResult(int exitCode, string consoleOutput)
        {
            ExitCode = exitCode;
            ConsoleOutput = consoleOutput;
        }
        public string ConsoleOutput { get; }
        public int ExitCode { get; }
    }
}
