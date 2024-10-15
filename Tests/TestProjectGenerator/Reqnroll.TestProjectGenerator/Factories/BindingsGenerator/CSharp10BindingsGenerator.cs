using System;

namespace Reqnroll.TestProjectGenerator.Factories.BindingsGenerator;

public class CSharp10BindingsGenerator : CSharpBindingsGenerator
{
    protected override string GetLogFileContent(string pathToLogFile)
    {
        string logFileContent = base.GetLogFileContent(pathToLogFile);
        logFileContent = "#nullable disable" + Environment.NewLine + logFileContent;
        return logFileContent;
    }
}
