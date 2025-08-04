using System.Threading.Tasks;
using Io.Cucumber.Messages.Types;

namespace Reqnroll.Formatters.PubSub;

public interface ICucumberMessageBroker : IPublishMessage
{
    bool IsEnabled { get; }

    void Initialize();
    void FormatterInitialized(ICucumberMessageFormatter formatterSink, bool enabled);
}
