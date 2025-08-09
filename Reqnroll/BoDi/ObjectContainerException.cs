using System;
using System.Linq;

namespace Reqnroll.BoDi;

public class ObjectContainerException : Exception
{
    public ObjectContainerException(string message, Type[] resolutionPath) : base(GetMessage(message, resolutionPath))
    {
    }

    private static string GetMessage(string message, Type[] resolutionPath)
    {
        if (resolutionPath == null || resolutionPath.Length == 0)
            return message;

        return $"{message} (resolution path: {string.Join("->", resolutionPath.Select(t => t.FullName).ToArray())})";
    }
}
