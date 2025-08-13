using System;
using System.Linq;
using System.Reflection;

namespace Reqnroll.Generator.Interfaces;

public class TestGenerationError
{
    /// <summary>
    /// The (zero-based) line number of the error.
    /// </summary>
    public int Line { get; private set; }
    /// <summary>
    /// The (zero-based) position of the error within the <see cref="Line"/>.
    /// </summary>
    public int LinePosition { get; private set; }
    /// <summary>
    /// The error message.
    /// </summary>
    public string Message { get; private set; }

    public TestGenerationError(int line, int linePosition, string message)
    {
        Line = line;
        LinePosition = linePosition;
        Message = message;
    }

    public TestGenerationError(Exception exception) :
        this(0, 0, "Generation error: " + GetMessage(exception))
    {
    }

    private static string GetMessage(Exception ex)
    {
        return "Message: " + ex.Message + Environment.NewLine + 
               Environment.NewLine +
               "AppDomain Information: " + Environment.NewLine +
               $"\tName: {AppDomain.CurrentDomain.FriendlyName}" + Environment.NewLine + 
               $"\tBaseDirectory: {AppDomain.CurrentDomain.BaseDirectory}" + Environment.NewLine +
               Environment.NewLine +
               "Loaded Assemblies:" + Environment.NewLine +
               "Fullname | Location" + Environment.NewLine +
               string.Join(Environment.NewLine, AppDomain.CurrentDomain.GetAssemblies().Select(a => $"{a.FullName} | {TryGetLocation(a)}").OrderBy(s => s)) + Environment.NewLine +
               Environment.NewLine +
               ex;
    }

    private static string TryGetLocation(Assembly a)
    {
        try
        {
            return a.Location;
        }
        catch (Exception)
        {
            return "unknown";
        }
    }
}