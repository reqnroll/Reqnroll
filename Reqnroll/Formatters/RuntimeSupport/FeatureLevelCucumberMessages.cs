using Io.Cucumber.Messages.Types;
using Reqnroll.Formatters.PayloadProcessing;
using System;
using System.Collections.Generic;
using System.Diagnostics;
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
    private Lazy<IEnumerable<Envelope>> _embeddedEnvelopes;
    internal readonly int _expectedEnvelopeCount;
    private Lazy<Source> _source;
    private Lazy<GherkinDocument> _gherkinDocument;
    private Lazy<IEnumerable<Pickle>> _pickles;

    public FeatureLevelCucumberMessages(string resourceNameOfEmbeddedMessages, int envelopeCount)
    {
        if (string.IsNullOrEmpty(resourceNameOfEmbeddedMessages))
            throw new ArgumentNullException(nameof(resourceNameOfEmbeddedMessages));

        var assm = Assembly.GetCallingAssembly();
        _embeddedEnvelopes = new Lazy<IEnumerable<Envelope>>(() => ReadEnvelopesFromAssembly(assm, resourceNameOfEmbeddedMessages));
        _expectedEnvelopeCount = envelopeCount;

        InitializeLazyProperties();
    }

    // Internal constructor for testing with direct stream access
    internal FeatureLevelCucumberMessages(Stream stream, string resourceNameOfEmbeddedMessages, int envelopeCount)
    {
        _embeddedEnvelopes = new Lazy<IEnumerable<Envelope>>(() => ReadEnvelopesFromStream(stream, resourceNameOfEmbeddedMessages));
        _expectedEnvelopeCount = envelopeCount;

        InitializeLazyProperties();
    }

    // Internal constructor for testing with pre-loaded envelopes
    internal FeatureLevelCucumberMessages(IEnumerable<Envelope> envelopes, int expectedEnvelopeCount)
    {
        _embeddedEnvelopes = new Lazy<IEnumerable<Envelope>>(() => envelopes);
        _expectedEnvelopeCount = expectedEnvelopeCount;

        InitializeLazyProperties();
    }
    private void InitializeLazyProperties()
    {
        _source = new Lazy<Source>(() => _embeddedEnvelopes.Value.Select(e => e.Source).DefaultIfEmpty(null).FirstOrDefault(s => s != null));
        _gherkinDocument = new Lazy<GherkinDocument>(() => _embeddedEnvelopes.Value.Select(e => e.GherkinDocument).DefaultIfEmpty(null).FirstOrDefault(g => g != null));
        _pickles = new Lazy<IEnumerable<Pickle>>(() => _embeddedEnvelopes.Value.Select(e => e.Pickle).Where(p => p != null));
    }

    internal IEnumerable<Envelope> ReadEnvelopesFromAssembly(Assembly assembly, string resourceNameOfEmbeddedNdJson)
    {
        var targetResourceName = resourceNameOfEmbeddedNdJson.Replace("\\", "/") + ".ndjson";

        try
        {
            using var stream = assembly.GetManifestResourceStream(targetResourceName);
            if (stream != null)
            {
                return ReadEnvelopesFromStream(stream, resourceNameOfEmbeddedNdJson);
            }
        }
        catch (System.Exception)
        {
            return Enumerable.Empty<Envelope>();
        }
        return Enumerable.Empty<Envelope>();
    }

    internal IEnumerable<Envelope> ReadEnvelopesFromStream(Stream stream, string resourceNameOfEmbeddedNdJson)
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
            return Enumerable.Empty<Envelope>();
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
            return envelopeCount > 0 && envelopeCount == _expectedEnvelopeCount;
        }
    }

    public Source Source => _source.Value;
    public GherkinDocument GherkinDocument => _gherkinDocument.Value;
    public IEnumerable<Pickle> Pickles => _pickles.Value;
}