using System;
using System.Threading.Tasks;
using Io.Cucumber.Messages.Types;

namespace Reqnroll.Formatters.PubSub;

public interface ICucumberMessageBroker
{
    bool Enabled { get; }
    Task PublishAsync(Envelope featureMessages);

    void RegisterSink(ICucumberMessageSink sink);
    void SinkInitialized(ICucumberMessageSink formatterSink, bool enabled);

    public event EventHandler<BrokerReadyEventArgs> BrokerReadyEvent;
}
