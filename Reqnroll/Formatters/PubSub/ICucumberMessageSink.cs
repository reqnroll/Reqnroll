using Io.Cucumber.Messages.Types;
using System.Threading.Tasks;


namespace Reqnroll.Formatters.PubSub
{
    public interface ICucumberMessageSink
    {
        Task PublishAsync(Envelope message);
    }
}
