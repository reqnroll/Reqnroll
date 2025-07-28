using System.Threading.Tasks;
using Io.Cucumber.Messages.Types;

namespace Reqnroll.Formatters.PubSub;

public interface IPublishMessage
{
    Task PublishAsync(Envelope featureMessages);
}
