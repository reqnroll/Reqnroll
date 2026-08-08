#nullable enable

using OpenTelemetry.Resources;
using System;
using System.Collections.Generic;

namespace Reqnroll.Diagnostics;

/// <summary>
/// Detects information about the container environment in which the application is running.
/// </summary>
/// <remarks>
/// <para>This detector provides the following attributes:</para>
/// <list type="table">
///   <item>
///     <term>container.runtime</term>
///     <description>The container runtime, if Reqnroll is running in a container.</description>
///   </item>
/// </list>
/// </remarks>
public class ContainerDetector : IResourceDetector
{
    public Resource Detect()
    {
        var attributes = new List<KeyValuePair<string, object>>();

        var inContainer = bool.TryParse(
            Environment.GetEnvironmentVariable("DOTNET_RUNNING_IN_CONTAINER"),
            out bool value) && value;

        if (inContainer)
        {
            attributes.Add(new("container.runtime", "docker"));
        }

        return new Resource(attributes);
    }
}
