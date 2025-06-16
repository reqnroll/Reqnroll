using Io.Cucumber.Messages.Types;
using System.Threading.Tasks;


namespace Reqnroll.Formatters.PubSub
{
    public interface ICucumberMessageSink
    {
        string Name { get; }
        Task PublishAsync(Envelope message);
    }
}
