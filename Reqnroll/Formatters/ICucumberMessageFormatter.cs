using Io.Cucumber.Messages.Types;
using Reqnroll.Formatters.Configuration;
using Reqnroll.Formatters.PubSub;
using System.Threading.Tasks;

namespace Reqnroll.Formatters;

public interface ICucumberMessageFormatter
{
    void LaunchFormatter(ICucumberMessageBroker broker);
    string Name { get; }
    AttachmentHandlingOption AttachmentHandlingOption { get; }
    Task PublishAsync(Envelope message);
}