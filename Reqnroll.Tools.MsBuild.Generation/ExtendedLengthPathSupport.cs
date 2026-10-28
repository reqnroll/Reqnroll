using System;
using System.IO;

namespace Reqnroll.Tools.MsBuild.Generation;

/// <summary>
/// Detects whether the current process accepts extended-length (<c>\\?\</c>) paths.
/// </summary>
/// <remarks>
/// With .NET Framework legacy path handling (<c>Switch.System.IO.UseLegacyPathHandling=true</c>) extended-length paths
/// are rejected with "Illegal characters in path" and long paths are not supported at all. This can happen in MSBuild
/// processes we do not control (e.g. x86 MSBuild.exe 18.11 on some machines),
/// see https://github.com/reqnroll/Reqnroll/issues/1128.
/// We test the actual behavior instead of checking the switch, so that it works regardless of how the setting was made.
/// </remarks>
public static class ExtendedLengthPathSupport
{
    private static readonly Lazy<bool> _isSupported = new(DetectSupport);

    /// <summary>
    /// Whether the current process accepts extended-length paths. The check is performed only once per process.
    /// </summary>
    public static bool IsSupported => _isSupported.Value;

    private static bool DetectSupport()
    {
        try
        {
            // does not access the file system, only validates and normalizes the path
            Path.GetFullPath(@"\\?\" + Path.GetTempPath());
            return true;
        }
        catch (ArgumentException)
        {
            return false;
        }
        catch (NotSupportedException)
        {
            return false;
        }
    }
}
