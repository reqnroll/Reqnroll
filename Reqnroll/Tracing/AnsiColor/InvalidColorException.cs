#nullable enable
using System;

namespace Reqnroll.Tracing.AnsiColor;

public class InvalidColorException : Exception
{
    public InvalidColorException(string? message)
        : base(message)
    {
    }
}
