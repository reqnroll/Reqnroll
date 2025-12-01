using Io.Cucumber.Messages.Types;
using System.Collections;
using System.Collections.Generic;

namespace Reqnroll.Formatters.RuntimeSupport;

public interface IFeatureLevelCucumberMessages
{
    bool HasMessages { get; }
    GherkinDocument GherkinDocument { get; }
    IEnumerable<Pickle> Pickles { get; }
    Source Source { get; }

    string GetPickleIndexFromTestRow(string featureName, string scenarioOutlineName, IEnumerable<string> tags, ICollection rowValues);
}