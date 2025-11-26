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
    private ConcurrentDictionary<string, (List<int>, int)> _rowHashToPickleIndexMaps;
    private Lazy<IEnumerable<Pickle>> _pickles;

    internal int ExpectedEnvelopeCount { get; }

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
    internal FeatureLevelCucumberMessages(Stream stream, string resourceNameOfEmbeddedMessages, int envelopeCount)
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

        _rowHashToPickleIndexMaps = new ConcurrentDictionary<string, (List<int>, int)>();

        var pickles = _embeddedEnvelopes.Value.Select(e => e.Pickle).Where(p => p != null).ToArray();
        for (int i = 0; i < pickles.Count(); i++)
        {
            var p = pickles[i];
            if (TestRowPickleMapper.PickleHasRowHashMarkerTag(p, out var rowHash))
            {
                var hashUseTracker = _rowHashToPickleIndexMaps.GetOrAdd(rowHash, (new List<int>(), 0));
                hashUseTracker.Item1.Add(i);
                _rowHashToPickleIndexMaps[rowHash] = hashUseTracker;
                TestRowPickleMapper.RemoveHashRowMarkerTag(p);
            }
        }

        _pickles = new Lazy<IEnumerable<Pickle>>(() => pickles);
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
    public IEnumerable<Pickle> Pickles => _pickles.Value;

    public string GetPickleIndexFromTestRow(string featureName, string scenarioOutlineName, IEnumerable<string> tags, ICollection rowValues)
    {
        var rowValuesStrings = rowValues.Cast<object>().Select(v => v?.ToString() ?? string.Empty);

        var rowHash = TestRowPickleMapper.ComputeHash(featureName, scenarioOutlineName, tags, rowValuesStrings);

        // The _rowHashToPickleIndexMaps dictionary maps row hashes to a tuple of (List of pickle indices, current use index).
        // We will round-robin through the list of pickle indices for each row hash as they are requested.

        // Thread-safe update of useIndex using a loop and TryUpdate
        if (_rowHashToPickleIndexMaps.TryGetValue(rowHash, out var hashUseTracker))
        {
            var (pickleIndices, useIndex) = hashUseTracker;
            var pickleIndex = pickleIndices[useIndex];
            var newUseCount = (useIndex + 1) % pickleIndices.Count;

            // Try to update the useIndex atomically
            while (!_rowHashToPickleIndexMaps.TryUpdate(rowHash, (pickleIndices, newUseCount), hashUseTracker))
            {
                // If update failed, reload and retry
                if (!_rowHashToPickleIndexMaps.TryGetValue(rowHash, out hashUseTracker))
                    break;
                (pickleIndices, useIndex) = hashUseTracker;
                pickleIndex = pickleIndices[useIndex];
                newUseCount = (useIndex + 1) % pickleIndices.Count;
            }

            return pickleIndex.ToString();
        }

        return null;
    }

}