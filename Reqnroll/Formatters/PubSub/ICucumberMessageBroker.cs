using System;
using System.Threading.Tasks;
using Io.Cucumber.Messages.Types;

namespace Reqnroll.Formatters.PubSub;

public interface ICucumberMessageBroker
{
    bool IsEnabled { get; }
    Task PublishAsync(Envelope featureMessages);
    void SinkInitialized(ICucumberMessageSink formatterSink, bool enabled);
}
