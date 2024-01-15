namespace TechTalk.SpecFlow.TestProjectGenerator
{
    public class CompileResult
    {
        public CompileResult(int exitCode, string output)
        {
            ExitCode = exitCode;
            IsSuccessful = exitCode == 0;
            Output = output;
        }

        public int ExitCode { get; }
        public bool IsSuccessful { get; }
        public string Output { get; }
    }
}