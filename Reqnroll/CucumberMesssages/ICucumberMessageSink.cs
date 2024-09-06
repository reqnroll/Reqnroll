using System.Threading.Tasks;


namespace Reqnroll.CucumberMessages
{
    public interface ICucumberMessageSink
    {
        void Publish(ReqnrollCucumberMessage message);
    }
}
