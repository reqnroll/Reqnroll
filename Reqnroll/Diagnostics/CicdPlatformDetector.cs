#nullable enable

using OpenTelemetry.Resources;
using Reqnroll.EnvironmentAccess;
using System.Collections.Generic;

namespace Reqnroll.Diagnostics;

/// <summary>
/// Detects the CI/CD platform the process is running inside of.
/// </summary>
/// <remarks>
/// <para>This detector provides the following attributes:</para>
/// <list type="table">
///   <item>
///     <term>cicd.platform.name</term>
///     <description>The name of the CI/CD platform.</description>
///   </item>
/// </list>
/// </remarks>
public class CicdPlatformDetector : IResourceDetector
{
    private readonly EnvironmentInfoProvider _environmentInfoProvider = new(new EnvironmentWrapper());

    /// <summary>
    /// Detects the CI/CD platform.
    /// </summary>
    /// <returns>A <see cref="Resource"/> representing the CI/CD platform.</returns>
    public Resource Detect()
    {
        var cicdPlatformName = _environmentInfoProvider.GetBuildServerName();

        var attributes = new List<KeyValuePair<string, object>>();

        if (cicdPlatformName is not null)
        {
            attributes.Add(new("cicd.platform.name", cicdPlatformName));
        }

        return new Resource(attributes);
    }
}
