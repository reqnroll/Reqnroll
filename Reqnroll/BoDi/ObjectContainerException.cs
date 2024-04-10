using System;
using System.Linq;
using System.Runtime.Serialization;

namespace Reqnroll.BoDi;

[Serializable]
public class ObjectContainerException : Exception
{
    public ObjectContainerException(string message, Type[] resolutionPath) : base(GetMessage(message, resolutionPath))
    {
    }

    protected ObjectContainerException(
        SerializationInfo info,
        StreamingContext context) : base(info, context)
    {
    }

    private static string GetMessage(string message, Type[] resolutionPath)
    {
        if (resolutionPath == null || resolutionPath.Length == 0)
            return message;

        return $"{message} (resolution path: {string.Join("->", resolutionPath.Select(t => t.FullName).ToArray())})";
    }
}
