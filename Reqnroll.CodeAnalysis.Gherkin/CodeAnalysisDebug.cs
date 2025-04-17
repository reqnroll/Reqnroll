﻿using System.Diagnostics;

namespace Reqnroll.CodeAnalysis.Gherkin;

internal static class CodeAnalysisDebug
{
    /// <inheritdoc cref="Debug.Assert(bool, string)"/>
    [Conditional("DEBUG")]
    public static void Assert(bool condition, string message) => Debug.Assert(condition, message);

    /// <inheritdoc cref="Debug.Assert(bool, string, string, object[])"/>
    [Conditional("DEBUG")]
    public static void Assert(bool condition, string message, string detailMessageFormat, params object[] args) => 
        Debug.Assert(condition, message, detailMessageFormat, args);
}
