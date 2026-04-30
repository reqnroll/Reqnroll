using System.Threading.Tasks;
using Io.Cucumber.Messages.Types;
using Reqnroll.Formatters.Configuration;

namespace Reqnroll.Formatters.PubSub;

public interface IMessagePublisher
{
    Task PublishAsync(Envelope message);
    AttachmentHandlingOption AggregateAttachmentHandlingOption { get; }
}
