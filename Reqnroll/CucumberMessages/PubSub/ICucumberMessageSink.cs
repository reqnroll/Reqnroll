using System.Threading.Tasks;


namespace Reqnroll.CucumberMessages.PubSub
{
    public interface ICucumberMessageSink
    {
        Task PublishAsync(ReqnrollCucumberMessage message);
    }
}
