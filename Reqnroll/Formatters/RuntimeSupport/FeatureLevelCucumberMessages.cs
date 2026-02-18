using Io.Cucumber.Messages.Types;
using Reqnroll.Formatters.PayloadProcessing;
using System;
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
        _pickles = new Lazy<IEnumerable<Pickle>>(() => _embeddedEnvelopes.Value.Select(e => e.Pickle).Where(p => p != null));
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
}