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

    public FeatureLevelCucumberMessages(string resourceNameOfEmbeddedNdJson)
    {
        var assm = Assembly.GetCallingAssembly();
        _embeddedEnvelopes = new Lazy<IEnumerable<Envelope>>(() => ReadEnvelopesFromAssembly(assm, resourceNameOfEmbeddedNdJson));
    }

    private IEnumerable<Envelope> ReadEnvelopesFromAssembly(Assembly assembly, string resourceNameOfEmbeddedNdJson)
    {
        var targetResourceName = resourceNameOfEmbeddedNdJson.Replace("/", "\\") + ".ndjson";
        var resourceName = assembly
            .GetManifestResourceNames()
            .FirstOrDefault(n => n.EndsWith(targetResourceName, StringComparison.OrdinalIgnoreCase));

        if (resourceName != null)
        {
            using (var stream = assembly.GetManifestResourceStream(resourceName))
            {
                if (stream != null)
                    using (var sr = new StreamReader(stream))
                    {
                        var content = sr.ReadToEnd();
                        var lines = content.Split(new[] { "\r\n", "\n", "\r" }, StringSplitOptions.RemoveEmptyEntries);
                        return lines.Select(l => NdjsonSerializer.Deserialize(l)).ToList();
                    }
            }
        }
        return Enumerable.Empty<Envelope>();
    }


    public bool HasMessages => Source is not null && GherkinDocument is not null;
    public Source Source => _embeddedEnvelopes.Value.Select(e => e.Source).FirstOrDefault(s => s != null);
    public GherkinDocument GherkinDocument => _embeddedEnvelopes.Value.Select(e => e.GherkinDocument).FirstOrDefault(g => g != null);
    public IEnumerable<Pickle> Pickles => _embeddedEnvelopes.Value.Select(e => e.Pickle).Where(p => p != null);
}