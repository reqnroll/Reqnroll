using Io.Cucumber.Messages.Types;
using System.Collections.Generic;

namespace Reqnroll.Formatters.RuntimeSupport;

public interface IFeatureLevelCucumberMessages
{
    bool HasMessages { get; }
    GherkinDocument GherkinDocument { get; }
    IEnumerable<Pickle> Pickles { get; }
    Source Source { get; }
}