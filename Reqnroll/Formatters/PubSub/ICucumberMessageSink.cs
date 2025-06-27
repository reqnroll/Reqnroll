using Io.Cucumber.Messages.Types;
using System.Threading.Tasks;

namespace Reqnroll.Formatters.PubSub;

public interface ICucumberMessageSink
{
    void LaunchSink();
    string Name { get; }
    Task PublishAsync(Envelope message);
}