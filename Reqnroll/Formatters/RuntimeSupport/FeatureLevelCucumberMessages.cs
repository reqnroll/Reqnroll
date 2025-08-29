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
    private readonly int _expectedEnvelopeCount;

    public FeatureLevelCucumberMessages(string resourceNameOfEmbeddedMessages, int envelopeCount)
    {
        var assm = Assembly.GetCallingAssembly();
        _embeddedEnvelopes = new Lazy<IEnumerable<Envelope>>(() => ReadEnvelopesFromAssembly(assm, resourceNameOfEmbeddedMessages));
        _expectedEnvelopeCount = envelopeCount;
    }

    private IEnumerable<Envelope> ReadEnvelopesFromAssembly(Assembly assembly, string resourceNameOfEmbeddedNdJson)
    {
        var targetResourceName = resourceNameOfEmbeddedNdJson.Replace("\\", "/") + ".ndjson";

        try
        {
            IEnumerable<string> ReadAllLines(StreamReader reader)
            {
                while (reader.ReadLine() is { } line)
                    yield return line;
            }

            if (targetResourceName != null)
            {
                using var stream = assembly.GetManifestResourceStream(targetResourceName);
                if (stream != null)
                {
                    using var sr = new StreamReader(stream);
                    return ReadAllLines(sr).Select(NdjsonSerializer.Deserialize).ToArray();
                }
            }
        }
        catch (System.Exception)
        {
            return Enumerable.Empty<Envelope>();
        }
        return Enumerable.Empty<Envelope>();
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

    public Source Source => _embeddedEnvelopes.Value.Select(e => e.Source).FirstOrDefault(s => s != null);
    public GherkinDocument GherkinDocument => _embeddedEnvelopes.Value.Select(e => e.GherkinDocument).FirstOrDefault(g => g != null);
    public IEnumerable<Pickle> Pickles => _embeddedEnvelopes.Value.Select(e => e.Pickle).Where(p => p != null);
}