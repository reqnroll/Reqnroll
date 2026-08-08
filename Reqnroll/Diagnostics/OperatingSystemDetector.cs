#nullable enable

using OpenTelemetry.Resources;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Xml.Linq;

namespace Reqnroll.Diagnostics;

/// <summary>
/// Resource detector for operating system information.
/// </summary>
/// <remarks>
/// <para>This detector provides the following attributes:</para>
/// <list type="table">
///   <item>
///     <term>os.type</term>
///     <description>The operating system type. Typical values:
///       <list type="list">
///         <item>windows</item>
///         <item>linux</item>
///         <item>darwin</item>
///       </list>
///     </description>
///   </item>
///   <item>
///     <term>os.version</term>
///     <description>The version string of the operating system.</description>
///   </item>
/// </list>
/// </remarks>
public class OperatingSystemDetector : IResourceDetector
{
    /// <summary>
    /// Detects the operating system resource.
    /// </summary>
    /// <returns>A <see cref="Resource"/> representing the operating system.</returns>
    public Resource Detect() => new(GetOsAttributes());

    private static Dictionary<string, object> GetOsAttributes()
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            return WindowsAttributes();
        }

        if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
        {
            return LinuxAttributes();
        }

        if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
        {
            return OsxAttributes();
        }

        return [];
    }

    private static Dictionary<string, object> WindowsAttributes()
    {
        var attributes = new Dictionary<string, object>
        {
            ["os.type"] = "windows"
        };

        var version = Environment.OSVersion;
        if (version.Version.Major > 0 || version.Version.Minor > 0 || version.Version.Build > 0)
        {
            attributes["os.version"] = version.Version.ToString(3); // Major.Minor.Build
        }

        var description = TryGetWindowsDescription();
        if (description != null)
        {
            attributes["os.description"] = description;
        }

        return attributes;
    }

    private static string? TryGetWindowsDescription()
    {
        try
        {
            return Environment.OSVersion.VersionString;
        }
        catch
        {
            return null;
        }
    }

    private static Dictionary<string, object> LinuxAttributes()
    {
        var attributes = new Dictionary<string, object>();

        var osReleaseInfo = LoadOsReleaseInfo();

        attributes["os.type"] = DetermineOsType(osReleaseInfo);

        if (osReleaseInfo.TryGetValue("PRETTY_NAME", out var prettyName))
        {
            attributes["os.description"] = prettyName;
        }

        if (osReleaseInfo.TryGetValue("NAME", out var name))
        {
            attributes["os.name"] = name;
        }

        if (osReleaseInfo.TryGetValue("VERSION_ID", out var versionId))
        {
            attributes["os.version"] = versionId;
        }
        else if (osReleaseInfo.TryGetValue("VERSION", out var version))
        {
            attributes["os.version"] = version;
        }

        if (osReleaseInfo.TryGetValue("BUILD_ID", out var buildId))
        {
            attributes["os.build_id"] = buildId;
        }

        return attributes;
    }

    private static string DetermineOsType(Dictionary<string, string> osReleaseInfo)
    {
        // Check the ID field to see if it's a well-known OS type
        if (osReleaseInfo.TryGetValue("ID", out var id))
        {
            var normalizedId = id.ToLowerInvariant();
            if (IsWellKnownOsType(normalizedId) && normalizedId != "linux")
            {
                return normalizedId;
            }

            // Check ID_LIKE field for parent OS types
            if (osReleaseInfo.TryGetValue("ID_LIKE", out var idLike))
            {
                var parentIds = idLike.Split([' '], StringSplitOptions.RemoveEmptyEntries);
                foreach (var parentId in parentIds)
                {
                    var normalized = parentId.ToLowerInvariant();
                    if (IsWellKnownOsType(normalized) && normalized != "linux")
                    {
                        return normalized;
                    }
                }
            }
        }

        // Default to linux if no other well-known type detected
        return "linux";
    }

    private static Dictionary<string, string> LoadOsReleaseInfo()
    {
        try
        {
            string? filePath = null;

            // Try standard locations
            if (File.Exists("/etc/os-release"))
            {
                filePath = "/etc/os-release";
            }
            else if (File.Exists("/usr/lib/os-release"))
            {
                filePath = "/usr/lib/os-release";
            }

            if (filePath == null)
            {
                return [];
            }

            var lines = File.ReadAllLines(filePath);

            return ParseOsReleaseContent(lines);
        }
        catch
        {
            // Ignore errors
        }

        return [];
    }

    private static Dictionary<string, string> ParseOsReleaseContent(string[] lines)
    {
        var result = new Dictionary<string, string>();

        foreach (var line in lines)
        {
            // Skip over empty lines and comments
            if (string.IsNullOrWhiteSpace(line) || line.StartsWith("#"))
            {
                continue;
            }

            // Ignore lines that don't contain an '=' character
            var eqIndex = line.IndexOf('=');
            if (eqIndex <= 0)
            {
                continue;
            }

            // Split the line into key and value, trimming whitespace and any surrounding quotes from the value
            var key = line.Substring(0, eqIndex).Trim();
            var value = line.Substring(eqIndex + 1).Trim();

            if ((value.StartsWith("\"") && value.EndsWith("\"")) ||
                (value.StartsWith("'") && value.EndsWith("'")))
            {
                value = value.Substring(1, value.Length - 2);
            }

            result[key] = value;
        }

        return result;
    }

    private static Dictionary<string, object> OsxAttributes()
    {
        var attributes = new Dictionary<string, object>
        {
            ["os.type"] = "darwin"
        };

        var systemVersionInfo = LoadSystemVersion();

        if (systemVersionInfo.TryGetValue("ProductName", out var productName))
        {
            attributes["os.name"] = productName;
        }

        if (systemVersionInfo.TryGetValue("ProductUserVisibleVersion", out var userVersion))
        {
            attributes["os.version"] = userVersion;
        }

        if (systemVersionInfo.TryGetValue("ProductBuildVersion", out var buildVersion))
        {
            attributes["os.build_id"] = buildVersion;
        }

        var description = TryBuildOsxDescription(systemVersionInfo);
        if (description != null)
        {
            attributes["os.description"] = description;
        }

        return attributes;
    }

    private static Dictionary<string, string> LoadSystemVersion()
    {
        try
        {
            string? plistPath = null;

            if (File.Exists("/System/Library/CoreServices/SystemVersion.plist"))
            {
                plistPath = "/System/Library/CoreServices/SystemVersion.plist";
            }
            else if (File.Exists("/System/Library/CoreServices/ServerVersion.plist"))
            {
                plistPath = "/System/Library/CoreServices/ServerVersion.plist";
            }

            if (plistPath == null)
            {
                return [];
            }

            var content = File.ReadAllText(plistPath);

            return ParsePlistProperties(content);
        }
        catch
        {
            // Ignore errors
        }

        return [];
    }

    private static Dictionary<string, string> ParsePlistProperties(string content)
    {
        // .plist is an XML format for representing key-value pairs.
        var result = new Dictionary<string, string>();

        try
        {
            var doc = XDocument.Parse(content);
            var root = doc.Root;

            if (root == null)
            {
                return result;
            }

            var dict = root.Element("dict");
            if (dict == null)
            {
                return result;
            }

            var elements = dict.Elements().ToList();
            for (int i = 0; i < elements.Count - 1; i++)
            {
                var element = elements[i];

                if (element.Name.LocalName == "key")
                {
                    var key = element.Value;
                    var nextElement = elements[i + 1];

                    if (nextElement.Name.LocalName == "string")
                    {
                        var value = nextElement.Value;
                        result[key] = value;
                    }
                }
            }
        }
        catch
        {
            // Ignore errors
        }

        return result;
    }

    private static string? TryBuildOsxDescription(Dictionary<string, string> versionInfo)
    {
        try
        {
            if (!versionInfo.TryGetValue("ProductName", out var productName))
            {
                return null;
            }

            var parts = new List<string> { productName };

            if (versionInfo.TryGetValue("ProductUserVisibleVersion", out var version))
            {
                parts.Add(version);
            }

            if (versionInfo.TryGetValue("ProductBuildVersion", out var build))
            {
                parts.Add($"({build})");
            }

            return string.Join(" ", parts);
        }
        catch
        {
            return null;
        }
    }

    /// <summary>
    /// Determines whether an OS type value is on the well-known list defined by the OpenTelemetry specification.
    /// </summary>
    /// <param name="osType">The value to check.</param>
    /// <returns><c>true</c> if the value is a well-known identifier; otherwise <c>false</c>.</returns>
    /// <remarks>
    /// Operating system spsecification is found at https://opentelemetry.io/docs/specs/semconv/resource/os/
    /// </remarks>
    private static bool IsWellKnownOsType(string osType)
    {
        return osType switch
        {
            "aix" => true,
            "darwin" => true,
            "dragonflybsd" => true,
            "freebsd" => true,
            "hpux" => true,
            "linux" => true,
            "netbsd" => true,
            "openbsd" => true,
            "solaris" => true,
            "windows" => true,
            "zos" => true,
            _ => false
        };
    }
}
