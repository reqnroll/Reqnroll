using Reqnroll.BoDi;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Security.Authentication.ExtendedProtection;
using System.Text;
using System.Threading.Tasks;


namespace Reqnroll.CucumberMesssages
{

    public interface ICucumberMessageBroker
    {
        Task CompleteAsync(string cucumberMessageSource);
        Task PublishAsync(ReqnrollCucumberMessage message);
    }

    public class CucumberMessageBroker : ICucumberMessageBroker
    {
        private List<ICucumberMessageSink> registeredSinks;


        public CucumberMessageBroker(IObjectContainer objectContainer)
        {
            var sinks = objectContainer.ResolveAll<ICucumberMessageSink>();
            registeredSinks = new List<ICucumberMessageSink>(sinks);
        }
        public async Task PublishAsync(ReqnrollCucumberMessage message)
        {
            foreach (var sink in registeredSinks)
            {
                await sink.Publish(message);
            }
        }

        // using an empty CucumberMessage to indicate completion
        public async Task CompleteAsync(string cucumberMessageSource)
        {
            var completionMessage = new ReqnrollCucumberMessage
            {
                CucumberMessageSource = cucumberMessageSource
            };

            foreach (var sink in registeredSinks)
            {
                await sink.Publish(completionMessage);
            }
        }
    }
}
