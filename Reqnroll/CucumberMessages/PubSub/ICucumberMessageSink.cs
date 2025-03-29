using Io.Cucumber.Messages.Types;
using System.Threading.Tasks;


namespace Reqnroll.CucumberMessages.PubSub
{
    public interface ICucumberMessageSink
    {
        Task PublishAsync(Envelope message);
    }
}
