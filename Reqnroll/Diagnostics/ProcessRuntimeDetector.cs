#nullable enable

using OpenTelemetry.Resources;
using System;
using System.Runtime.InteropServices;

namespace Reqnroll.Diagnostics;

/// <summary>
/// Resource detector for process information about the .NET runtime.
/// </summary>
public class ProcessRuntimeDetector : IResourceDetector
{
    /// <summary>
    /// Detects process information about the .NET runtime.
    /// </summary>
    /// <returns>A <see cref="Resource"/> with process runtime attributes.</returns>
    public Resource Detect()
    {
        return new Resource(
        [
            new("process.runtime.name", ".NET"),
            new("process.runtime.description", RuntimeInformation.FrameworkDescription),
            new("process.runtime.version", Environment.Version.ToString(3))
        ]);
    }
}
