using System;
using System.Linq;
using System.Text.RegularExpressions;

namespace Reqnroll.TestProjectGenerator;

public class CompileResult(int exitCode, string output)
{
    public int ExitCode { get; } = exitCode;

    public bool IsSuccessful { get; } = exitCode == 0;

    public string Output { get; } = output;

    public string ErrorLines =>
        string.Join(
            Environment.NewLine,
            Regex.Split(Output, @"\r?\n").Where(l => l.Contains("error")));
}