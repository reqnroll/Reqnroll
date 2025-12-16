using Io.Cucumber.Messages.Types;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Reqnroll.Formatters.RuntimeSupport;

public static class TestRowPickleMapper
{
    internal const string RowHashTagPrefix = "@__RowHash_";
    internal static string ComputeHash(string featureName, string scenarioOutlineName, IEnumerable<string> tags, IEnumerable<string> rowValues)
    {
        var tagsList = tags ?? Enumerable.Empty<string>();
        tagsList = tagsList.Select(t => t.StartsWith("@") ? t : $"@{t}");
        var rowValuesList = rowValues ?? Enumerable.Empty<string>();
        var v = $"{featureName}|{scenarioOutlineName}|{string.Join("|", tagsList)}|{string.Join("|", rowValuesList)}";

        // Use MD5 for a fast, 128-bit hash
        using var md5 = System.Security.Cryptography.MD5.Create();
        var inputBytes = System.Text.Encoding.UTF8.GetBytes(v);
        var hashBytes = md5.ComputeHash(inputBytes);
        // Convert to hex string
        return BitConverter.ToString(hashBytes).Replace("-", "").ToLowerInvariant();
    }

    public static void MarkPickleWithRowHash(Pickle pickle, string featureName, string scenarioOutlineName, IEnumerable<string> tags, IEnumerable<string> rowValues)
    {
        pickle.Tags.Add(new PickleTag($"{RowHashTagPrefix}{ComputeHash(featureName, scenarioOutlineName, tags, rowValues)}", ""));
    }

    internal static bool PickleHasRowHashMarkerTag(Pickle p, out string rowHash)
    {
        var tag = p.Tags.FirstOrDefault(t => t.Name.StartsWith(RowHashTagPrefix, StringComparison.OrdinalIgnoreCase));
        if (tag != null)
        {
            rowHash = tag.Name.Substring(RowHashTagPrefix.Length);
            return true;
        }
        rowHash = null;
        return false;
    }

    internal static void RemoveHashRowMarkerTag(Pickle p)
    {
        var tag = p.Tags.FirstOrDefault(t => t.Name.StartsWith(RowHashTagPrefix, StringComparison.OrdinalIgnoreCase));
        if (tag != null)
        {
            p.Tags.Remove(tag);
        }
    }
}

