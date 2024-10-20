using System.Threading.Tasks;


namespace Reqnroll.CucumberMessages.PubSub
{
    public interface ICucumberMessageSink
    {
        Task Publish(ReqnrollCucumberMessage message);
    }
}
