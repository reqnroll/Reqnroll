using Io.Cucumber.Messages.Types;
using System;
using System.Collections.Generic;

namespace Reqnroll.Formatters.RuntimeSupport;

/// <summary>
/// This class is used at Code Generation time to provide serialized representations of the Source, GherkinDocument, and Pickles 
/// to be used at runtime.
/// </summary>
public class FeatureLevelCucumberMessages(Func<Source> source, Func<GherkinDocument> gherkinDocument, Func<IEnumerable<Pickle>> pickles, string location)
{
    public string Location { get; } = location;
    public Func<Source> Source { get; } = source;
    public Func<GherkinDocument> GherkinDocument { get; } = gherkinDocument;
    public Func<IEnumerable<Pickle>> Pickles { get; } = pickles;
}