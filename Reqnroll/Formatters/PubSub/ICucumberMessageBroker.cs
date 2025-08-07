namespace Reqnroll.Formatters.PubSub;

public interface ICucumberMessageBroker : IMessagePublisher
{
    bool IsEnabled { get; }

    void Initialize();
    void FormatterInitialized(ICucumberMessageFormatter formatterSink, bool enabled);
}
