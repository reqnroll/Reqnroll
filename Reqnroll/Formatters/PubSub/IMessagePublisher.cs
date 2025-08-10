using System.Threading.Tasks;
using Io.Cucumber.Messages.Types;

namespace Reqnroll.Formatters.PubSub;

public interface IMessagePublisher
{
    Task PublishAsync(Envelope message);
}
