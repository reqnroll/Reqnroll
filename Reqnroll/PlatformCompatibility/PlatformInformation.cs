using System.Diagnostics;
using System.Runtime.InteropServices;
using System;
using System.Text.RegularExpressions;

namespace Reqnroll.PlatformCompatibility;
internal class PlatformInformation
{
    public static Architecture ProcessArchitecture => RuntimeInformation.ProcessArchitecture;
    public static string DotNetFullVersion { get; } = FileVersionInfo.GetVersionInfo(typeof(Uri).Assembly.Location).ProductVersion;
    public static string DotNetFrameworkDescription => RuntimeInformation.FrameworkDescription;
    public static string DotNetFrameworkMainDescription { get; } = GetMainDotNetFrameworkDescription();
    public static bool IsDotNetFramework => DotNetFrameworkDescription.StartsWith(".NET Framework", StringComparison.OrdinalIgnoreCase);

    private static string GetMainDotNetFrameworkDescription()
    {
        return Regex.Replace(RuntimeInformation.FrameworkDescription, @"(?<mver>\d+\.\d+)(\.\d+)*", "${mver}");
    }
}
