using System.Threading.Tasks;


namespace Reqnroll.CucumberMesssages
{
    public interface ICucumberMessageSink
    {
        void Publish(ReqnrollCucumberMessage message);
    }
}
