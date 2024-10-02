using Io.Cucumber.Messages.Types;


namespace Reqnroll.CucumberMessages.PubSub
{
    public class ReqnrollCucumberMessage
    {
        public string CucumberMessageSource { get; set; }
        public Envelope Envelope { get; set; }
    }
}
