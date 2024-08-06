using System.Threading.Tasks;


namespace Reqnroll.CucumberMesssages
{
    public interface ICucumberMessageSink
    {
        Task Publish(ReqnrollCucumberMessage message);
    }
}
