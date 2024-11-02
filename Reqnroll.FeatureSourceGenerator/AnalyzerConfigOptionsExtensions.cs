using Microsoft.CodeAnalysis.Diagnostics;

namespace Reqnroll.FeatureSourceGenerator;

internal static class AnalyzerConfigOptionsExtensions
{
    public static string? GetStringValue(this AnalyzerConfigOptions options, string name)
    {
        if (options.TryGetValue(name, out var value))
        {
            return string.IsNullOrEmpty(value) ? null : value;
        }

        return null;
    }

    public static string? GetStringValue(this AnalyzerConfigOptions options, string name1, string name2)
    {
        return GetStringValue(options, name1) ?? GetStringValue(options, name2);
    }

    public static string? GetStringValue(this AnalyzerConfigOptions options, string name1, string name2, string name3)
    {
        return GetStringValue(options, name1) ?? GetStringValue(options, name2) ?? GetStringValue(options, name3);
    }

    public static bool? GetBooleanValue(this AnalyzerConfigOptions options, string name)
    {
        if (options.TryGetValue(name, out var value) && !string.IsNullOrEmpty(value))
        {
            if (bool.TryParse(value, out bool result))
            {
                return result;
            }

            return false;
        }

        return null;
    } 

    public static bool? GetBooleanValue(this AnalyzerConfigOptions options, string name1, string name2)
    {
        return GetBooleanValue(options, name1) ?? GetBooleanValue(options, name2);
    }

    public static bool? GetBooleanValue(this AnalyzerConfigOptions options, string name1, string name2, string name3)
    {

        return GetBooleanValue(options, name1) ?? GetBooleanValue(options, name2) ?? GetBooleanValue(options, name3);
    }
}
