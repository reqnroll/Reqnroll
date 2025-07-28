namespace Reqnroll.Formatters.PubSub;
public interface IFormattersBroker : IPublishMessage
{
    bool IsEnabled { get; }

    void Initialize();
    void SinkInitialized(ICucumberMessageFormatter formatterSink, bool enabled);
}
