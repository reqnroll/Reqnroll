using Microsoft.CodeAnalysis.Diagnostics;

namespace Reqnroll.FeatureSourceGenerator;

internal static class AnalyzerConfigOptionsExtensions
{
    public static string? GetStringValue(this AnalyzerConfigOptions options, string name)
    {
        options.TryGetValue(name, out var value);
        return value;
    }

    public static string? GetStringValue(this AnalyzerConfigOptions options, string name1, string name2)
    {
        if (options.TryGetValue(name1, out var value) ||
            options.TryGetValue(name2, out value))
        {
            return value;
        }

        return null;
    }

    public static string? GetStringValue(this AnalyzerConfigOptions options, string name1, string name2, string name3)
    {
        if (options.TryGetValue(name1, out var value) ||
            options.TryGetValue(name2, out value) ||
            options.TryGetValue(name3, out value))
        {
            return value;
        }

        return null;
    }

    public static bool? GetBooleanValue(this AnalyzerConfigOptions options, string name)
    {
        if (options.TryGetValue(name, out var value))
        {
            if (bool.TryParse(value, out bool result))
            {
                return result;
            }
        }

        return null;
    } 

    public static bool? GetBooleanValue(this AnalyzerConfigOptions options, string name1, string name2)
    {
        if (options.TryGetValue(name1, out var value) ||
            options.TryGetValue(name2, out value))
        {
            if (bool.TryParse(value, out bool result))
            {
                return result;
            }
        }

        return null;
    }

    public static bool? GetBooleanValue(this AnalyzerConfigOptions options, string name1, string name2, string name3)
    {
        if (options.TryGetValue(name1, out var value) ||
            options.TryGetValue(name2, out value) ||
            options.TryGetValue(name3, out value))
        {
            if (bool.TryParse(value, out bool result))
            {
                return result;
            }
        }

        return null;
    }
}
