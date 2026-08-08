#nullable enable

using OpenTelemetry.Resources;
using System.Collections.Generic;
using System.Reflection;

namespace Reqnroll.Diagnostics;

/// <summary>
/// A resource detector for Reqnroll runtime information, expressed as the service being reported for.
/// </summary>
/// <remarks>
/// <para>This detector provides the following attributes:</para>
/// <list type="table">
///   <item>
///     <term>service.name</term>
///     <description>Always the value <c>"Reqnroll"</c></description>
///   </item>
///   <item>
///     <term>service.version</term>
///     <description>The version of Reqnroll being used.</description>
///   </item>
/// </list>
/// </remarks>
public class ReqnrollServiceDetector : IResourceDetector
{
    /// <summary>
    /// Detects the Reqnroll resource.
    /// </summary>
    /// <returns>A <see cref="Resource"/> describing the Reqnroll runtime.</returns>
    public Resource Detect()
    {
        var version = typeof(ScenarioContext).Assembly
            .GetCustomAttribute<AssemblyInformationalVersionAttribute>()?
            .InformationalVersion;

        var attributes = new List<KeyValuePair<string, object>>()
        {
            new("service.name", "Reqnroll")
        };

        if (version is not null)
        {
            attributes.Add(new("service.version", version));
        }

        return new Resource(attributes);
    }
}
