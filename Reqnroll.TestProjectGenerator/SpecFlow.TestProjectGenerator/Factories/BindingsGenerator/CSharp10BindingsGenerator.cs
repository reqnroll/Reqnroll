using TechTalk.SpecFlow.TestProjectGenerator.Data;

namespace TechTalk.SpecFlow.TestProjectGenerator.Factories.BindingsGenerator;

public class CSharp10BindingsGenerator : CSharpBindingsGenerator
{
    public override ProjectFile GenerateLoggerClass(string pathToLogFile)
    {
        string fileContent = $@"
using System;
using System.IO;
using System.Runtime.CompilerServices;
using System.Threading;

internal static class Log
{{
    private const string LogFileLocation = @""{pathToLogFile}"";

    private static void Retry(int number, Action action)
    {{
        try
        {{
            action();
        }}
        catch (Exception)
        {{
            var i = number - 1;

            if (i == 0)
                throw;

            Thread.Sleep(500);
            Retry(i, action);
        }}
    }}

    internal static void LogStep([CallerMemberName] string stepName = null!)
    {{
        Retry(5, () => WriteToFile($@""-> step: {{stepName}}{{Environment.NewLine}}""));
    }}

    internal static void LogHook([CallerMemberName] string stepName = null!)
    {{
        Retry(5, () => WriteToFile($@""-> hook: {{stepName}}{{Environment.NewLine}}""));
    }}

    static void WriteToFile(string line)
    {{
        using (FileStream fs = File.Open(LogFileLocation, FileMode.Append, FileAccess.Write, FileShare.None))
        {{
            byte[] bytes = System.Text.Encoding.UTF8.GetBytes(line);
            fs.Write(bytes, 0, bytes.Length);
            fs.Close();
        }}
    }}
}}";
        return new ProjectFile("Log.cs", "Compile", fileContent);
    }

}
