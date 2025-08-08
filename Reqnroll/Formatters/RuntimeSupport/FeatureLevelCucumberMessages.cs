using Io.Cucumber.Messages.Types;
using System;
using System.Collections.Generic;

namespace Reqnroll.Formatters.RuntimeSupport;

/// <summary>
/// This class is used at Code Generation time to provide serialized representations of the Source, GherkinDocument, and Pickles 
/// to be used at runtime.
/// </summary>
public class FeatureLevelCucumberMessages(Func<Source> source, Func<GherkinDocument> gherkinDocument, Func<IEnumerable<Pickle>> pickles)
{
    private Lazy<Source> _source = new Lazy<Source>(source);
    private Lazy<GherkinDocument> _gherkinDocument = new Lazy<GherkinDocument>(gherkinDocument);
    private Lazy<IEnumerable<Pickle>> _pickles = new Lazy<IEnumerable<Pickle>>(pickles);

    public bool HasMessages => Source is not null && GherkinDocument is not null && Pickles is not null;
    public Source Source { get => _source.Value; }
    public GherkinDocument GherkinDocument { get => _gherkinDocument.Value; }
    public IEnumerable<Pickle> Pickles { get => _pickles.Value; }
}