using System.Threading.Tasks;


namespace Reqnroll.CucumberMessages.PubSub
{
    public interface ICucumberMessageSink
    {
        void Publish(ReqnrollCucumberMessage message);
    }
}
