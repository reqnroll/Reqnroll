using Io.Cucumber.Messages.Types;
using System.Collections.Generic;

namespace Reqnroll.Formatters.RuntimeSupport
{
    public interface IFeatureLevelCucumberMessages
    {
        GherkinDocument GherkinDocument { get; }
        bool HasMessages { get; }
        IEnumerable<Pickle> Pickles { get; }
        Source Source { get; }
    }
}