using Io.Cucumber.Messages.Types;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Reqnroll.Formatters.RuntimeSupport;

public static class TestRowPickleMapper
{
    public static object ComputeHash(string featureName, string scenarioOutlineName, IEnumerable<string> tags, IEnumerable<string> rowValues)
    {
        var tagsList = tags ?? Enumerable.Empty<string>();
        tagsList = tagsList.Select(t => t.StartsWith("@") ? t : $"@{t}");
        var rowValuesList = rowValues ?? Enumerable.Empty<string>();
        var v = $"{featureName}|{scenarioOutlineName}|{string.Join("|", tagsList)}|{string.Join("|", rowValuesList)}";

        // Use MD5 for a fast, 128-bit hash
        using (var md5 = System.Security.Cryptography.MD5.Create())
        {
            var inputBytes = System.Text.Encoding.UTF8.GetBytes(v);
            var hashBytes = md5.ComputeHash(inputBytes);
            // Convert to hex string
            return BitConverter.ToString(hashBytes).Replace("-", "").ToLowerInvariant();
        }
    }

    public static void MarkPickleWithRowHash(Pickle pickle, string featureName, string scenarioOutlineName, IEnumerable<string> tags, IEnumerable<string> rowValues)
    {
        pickle.Tags.Add(new Io.Cucumber.Messages.Types.PickleTag($"@RowHash_{ComputeHash(featureName, scenarioOutlineName, tags, rowValues)}", ""));
    }

    public static string GetPickleIndexFromTestRow(string featureName, string scenarioOutlineName, IEnumerable<string> tags, ICollection rowValues, IEnumerable<Pickle> pickles)
    {
        var rowValuesStrings = rowValues.Cast<object>().Select(v => v?.ToString() ?? string.Empty);

        var rowHash = ComputeHash(featureName, scenarioOutlineName, tags, rowValuesStrings);
        var tagName = $"@RowHash_{rowHash}";
        for (int i = 0; i < pickles.Count(); i++)
        {
            var pickle = pickles.ElementAt(i);
            // at this point, if the pickle has a tag with the row hash, we found it; 
            // in a thread safe way, remove the tag so that subsequent calls do not find the same pickle again
            lock (pickle.Tags)
            {
                var tag = pickle.Tags.FirstOrDefault(t => t.Name == tagName);
                if (tag != null)
                {
                    pickle.Tags.Remove(tag);
                    return i.ToString();
                }
            }
        }
        return null;
    }
}

