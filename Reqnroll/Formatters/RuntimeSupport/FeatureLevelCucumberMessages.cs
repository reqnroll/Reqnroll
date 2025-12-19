using Io.Cucumber.Messages.Types;
using Reqnroll.Formatters.PayloadProcessing;
using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace Reqnroll.Formatters.RuntimeSupport;

/// <summary>
/// This class is used at Code Generation time to provide serialized representations of the Source, GherkinDocument, and Pickles 
/// to be used at runtime.
/// </summary>
public class FeatureLevelCucumberMessages : IFeatureLevelCucumberMessages
{
    private readonly Lazy<IReadOnlyCollection<Envelope>> _embeddedEnvelopes;
    private Lazy<Source> _source;
    private Lazy<GherkinDocument> _gherkinDocument;
    private Lazy<(IEnumerable<Pickle> Pickles, ConcurrentDictionary<string, (int[] PickleIndices, int NextToBeUsed)> Map)> _pickleDetails;

    internal int ExpectedEnvelopeCount { get; }

    private ConcurrentDictionary<string, (int[] PickleIndices, int NextToBeUsed)> RowHashToPickleIndexMap => _pickleDetails.Value.Map;

    public FeatureLevelCucumberMessages(string featureMessagesResourceName, int envelopeCount)
    {
        var assembly = Assembly.GetCallingAssembly();

        var isEnabled = !string.IsNullOrEmpty(featureMessagesResourceName) && envelopeCount > 0;
        _embeddedEnvelopes = new Lazy<IReadOnlyCollection<Envelope>>(() =>
            isEnabled ? ReadEnvelopesFromAssembly(assembly, featureMessagesResourceName) : []);
        ExpectedEnvelopeCount = envelopeCount;

        InitializeLazyProperties();
    }

    // Internal constructor for testing with direct stream access
    internal FeatureLevelCucumberMessages(Stream stream, int envelopeCount)
    {
        _embeddedEnvelopes = new Lazy<IReadOnlyCollection<Envelope>>(() => ReadEnvelopesFromStream(stream));
        ExpectedEnvelopeCount = envelopeCount;

        InitializeLazyProperties();
    }

    // Internal constructor for testing with preloaded envelopes
    internal FeatureLevelCucumberMessages(IReadOnlyCollection<Envelope> envelopes, int expectedEnvelopeCount)
    {
        _embeddedEnvelopes = new Lazy<IReadOnlyCollection<Envelope>>(() => envelopes);
        ExpectedEnvelopeCount = expectedEnvelopeCount;

        InitializeLazyProperties();
    }

    private void InitializeLazyProperties()
    {
        _source = new Lazy<Source>(() => _embeddedEnvelopes.Value.Select(e => e.Source).DefaultIfEmpty(null).FirstOrDefault(s => s != null));
        _gherkinDocument = new Lazy<GherkinDocument>(() => _embeddedEnvelopes.Value.Select(e => e.GherkinDocument).DefaultIfEmpty(null).FirstOrDefault(g => g != null));

        (Pickle[], ConcurrentDictionary<string, (int[], int)>) CalculatePickleDetails()
        {
            var pickles = _embeddedEnvelopes.Value.Select(e => e.Pickle).Where(p => p != null).ToArray();

            var hashToIndexList = new List<(string Hash, int PickleIndex)>();

            for (int pickleIndex = 0; pickleIndex < pickles.Length; pickleIndex++)
            {
                var pickle = pickles[pickleIndex];
                if (TestRowPickleMapper.PickleHasRowHashMarkerTag(pickle, out var rowHash))
                {
                    TestRowPickleMapper.RemoveHashRowMarkerTag(pickle);
                    hashToIndexList.Add((rowHash, pickleIndex));
                }
            }

            // initialize the map with the indices per hash and set the next to be used to 0
            var map = new ConcurrentDictionary<string, (int[] PickleIndices, int NextToBeUsed)>(
                hashToIndexList
                    .GroupBy(e => e.Hash)
                    .Select(g => new KeyValuePair<string, (int[] PickleIndices, int NextToBeUsed)>(g.Key, (g.Select(e => e.PickleIndex).ToArray(), 0))));

            return (pickles, map);
        }

        _pickleDetails = new(() => CalculatePickleDetails());
    }

    internal IReadOnlyCollection<Envelope> ReadEnvelopesFromAssembly(Assembly assembly, string featureMessagesResourceName)
    {
        try
        {
            using var stream = assembly.GetManifestResourceStream(featureMessagesResourceName);
            if (stream != null)
            {
                return ReadEnvelopesFromStream(stream);
            }
        }
        catch (System.Exception)
        {
            return [];
        }
        return [];
    }

    internal IReadOnlyCollection<Envelope> ReadEnvelopesFromStream(Stream stream)
    {
        try
        {
            IEnumerable<string> ReadAllLines(StreamReader reader)
            {
                while (reader.ReadLine() is { } line)
                    yield return line;
            }

            using var sr = new StreamReader(stream);
            return ReadAllLines(sr).Select(NdjsonSerializer.Deserialize).ToArray();
        }
        catch (System.Exception)
        {
            return [];
        }
    }

    public bool HasMessages
    {
        get
        {
            var hasSourceAndGherkinDocument = Source is not null && GherkinDocument is not null;
            var envelopeCount = hasSourceAndGherkinDocument ?
                Pickles?.Count() + 2
                : 0;
            return envelopeCount > 0 && envelopeCount == ExpectedEnvelopeCount;
        }
    }

    public Source Source => _source.Value;
    public GherkinDocument GherkinDocument => _gherkinDocument.Value;
    public IEnumerable<Pickle> Pickles => _pickleDetails.Value.Pickles;

    public string GetPickleIndexFromTestRow(string featureName, string scenarioOutlineName, IEnumerable<string> tags, ICollection rowValues)
    {
        var rowValuesStrings = rowValues.Cast<object>().Select(v => v?.ToString() ?? string.Empty);

        var rowHash = TestRowPickleMapper.ComputeHash(featureName, scenarioOutlineName, tags, rowValuesStrings);

        // The _rowHashToPickleIndexMaps dictionary maps row hashes to a tuple of (List of pickle indices, current use index).
        // We will apply a round-robin through the list of pickle indices for each row hash as they are requested.

        // Thread-safe update of useIndex using a loop and TryUpdate
        if (RowHashToPickleIndexMap.TryGetValue(rowHash, out var hashUseTracker))
        {
            // returns the pickle index to use and calculates an updated hashUseTracker for the next use
            int AcquirePickleIndex(out (int[] PickleIndices, int NextToBeUsed) nextHashUseTracker)
            {
                var result = hashUseTracker.PickleIndices[hashUseTracker.NextToBeUsed];
                nextHashUseTracker = (hashUseTracker.PickleIndices, (hashUseTracker.NextToBeUsed + 1) % hashUseTracker.PickleIndices.Length);
                return result;
            }

            var pickleIndex = AcquirePickleIndex(out var newHashUseTracker);

            // Try to update the useIndex atomically
            while (!RowHashToPickleIndexMap.TryUpdate(rowHash, newHashUseTracker, hashUseTracker))
            {
                // If update failed, reload and retry
                if (!RowHashToPickleIndexMap.TryGetValue(rowHash, out hashUseTracker))
                    break;

                pickleIndex = AcquirePickleIndex(out newHashUseTracker);
            }

            return pickleIndex.ToString();
        }

        return null;
    }
}