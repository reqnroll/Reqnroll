using System.Reflection;

namespace Reqnroll;

internal class VersionInfo
{
    internal static string AssemblyVersion { get; }
    internal static string AssemblyFileVersion { get; }
    internal static string AssemblyInformationalVersion { get; }
    internal static string NuGetVersion { get; }

    static VersionInfo()
    {
        var assembly = Assembly.GetExecutingAssembly();
        AssemblyVersion = assembly.GetName().Version.ToString();
        AssemblyFileVersion = assembly.GetCustomAttribute<AssemblyFileVersionAttribute>()?.Version;
        AssemblyInformationalVersion = assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion;
        NuGetVersion = AssemblyInformationalVersion?.Split(new[] { '+' }, 2)[0];
    }
}