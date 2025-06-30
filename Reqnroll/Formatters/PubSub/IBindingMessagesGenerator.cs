using Io.Cucumber.Messages.Types;
using Reqnroll.Bindings;
using System.Collections.Generic;

namespace Reqnroll.Formatters.PubSub;

public interface IBindingMessagesGenerator
{
    IReadOnlyDictionary<IBinding, string> StepDefinitionIdByBinding { get; }
    IEnumerable<Envelope> StaticBindingMessages { get; }
    bool Ready { get; }
}

