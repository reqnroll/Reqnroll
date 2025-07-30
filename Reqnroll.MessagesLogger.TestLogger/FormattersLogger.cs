using Microsoft.VisualStudio.TestPlatform.ObjectModel.Client;
using System;
using System.Collections.Generic;

namespace Reqnroll.MessagesLogger.TestLogger;

public class FormattersLogger : ITestLoggerWithParameters
{
    public static bool IsInitialized { get; private set; } = false;
    public static bool HasParameters { get; private set; } = false;
    public static Dictionary<string, string> Parameters { get; private set; } = new();
    public static string TestRunDirectory { get; private set; } = string.Empty;
    public void Initialize(TestLoggerEvents events, Dictionary<string, string> parameters)
    {
        IsInitialized = true;
        HasParameters = parameters != null && parameters.Count > 0;
        Parameters = parameters;
    }

    public void Initialize(TestLoggerEvents events, string testRunDirectory)
    {
        TestRunDirectory = testRunDirectory;
        IsInitialized = true;
        HasParameters = false;
    }
}
