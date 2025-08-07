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
    public bool HasMessages => Source is not null && GherkinDocument is not null && Pickles is not null;
    public Source Source { get; } = source();
    public GherkinDocument GherkinDocument { get; } = gherkinDocument();
    public IEnumerable<Pickle> Pickles { get; } = pickles();
}