using System.Diagnostics;
using System.Runtime.InteropServices;
using System;
using System.Text.RegularExpressions;

namespace Reqnroll.PlatformCompatibility;
public class PlatformInformation
{
    public static Architecture ProcessArchitecture => RuntimeInformation.ProcessArchitecture;
    public static string DotNetFullVersion { get; } = FileVersionInfo.GetVersionInfo(typeof(Uri).Assembly.Location).ProductVersion;
    public static string DotNetFrameworkDescription => RuntimeInformation.FrameworkDescription;
    public static string DotNetFrameworkMainDescription { get; } = GetMainDotNetFrameworkDescription();
    public static bool IsDotNetFramework { get; } = GetIsDotNetFramework();

    private static bool GetIsDotNetFramework()
    {
        string frameworkDescription = DotNetFrameworkDescription;
        return frameworkDescription.StartsWith(".NET Framework", StringComparison.OrdinalIgnoreCase) ||
               frameworkDescription.StartsWith("Mono", StringComparison.OrdinalIgnoreCase);
    }

    private static string GetMainDotNetFrameworkDescription()
    {
        return Regex.Replace(RuntimeInformation.FrameworkDescription, @"(?<mver>\d+\.\d+).*", "${mver}");
    }
}
