using Io.Cucumber.Messages.Types;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Reqnroll.Formatters.RuntimeSupport;

public static class TestRowPickleMapper
{
    public static object ComputeHash(string featureName, string scenarioOutlineName, IEnumerable<string> tags, IEnumerable<string> rowValues)
    {
        var tagsList = tags ?? Enumerable.Empty<string>();
        var rowValuesList = rowValues ?? Enumerable.Empty<string>();
        var v = $"{featureName}|{scenarioOutlineName}|{string.Join("|", tagsList)}|{string.Join("|", rowValuesList)}";
        return v.GetHashCode();
    }

    public static void MarkPickleWithRowHash(IEnumerable<Envelope> envelopes, int pickleIndex, object rowHash)
    {
        var pickle = envelopes
            .Where(e => e.Pickle != null)
            .Select(e => e.Pickle)
            .ElementAt(pickleIndex);
        pickle.Tags.Add(new Io.Cucumber.Messages.Types.PickleTag($"@RowHash_{rowHash}", ""));
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

