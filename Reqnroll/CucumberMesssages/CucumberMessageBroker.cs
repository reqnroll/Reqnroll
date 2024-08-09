using Reqnroll.BoDi;
using Reqnroll.Tracing;
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
        private IObjectContainer _objectContainer;
        //private ITraceListener _traceListener;

        public CucumberMessageBroker(IObjectContainer objectContainer)
        { 
            _objectContainer = objectContainer;
            var sinks = objectContainer.ResolveAll<ICucumberMessageSink>();
            registeredSinks = new List<ICucumberMessageSink>(sinks);
        }
        public async Task PublishAsync(ReqnrollCucumberMessage message)
        {
            var _traceListener = _objectContainer.Resolve<ITraceListener>();
            _traceListener.WriteTestOutput("Broker publishing to " + registeredSinks.Count + " sinks");


            foreach (var sink in registeredSinks)
            {   
                _traceListener.WriteTestOutput($"Broker publishing {message.CucumberMessageSource}");

                await sink.Publish(message);
            }
        }

        // using an empty CucumberMessage to indicate completion
        public async Task CompleteAsync(string cucumberMessageSource)
        {
            var _traceListener = _objectContainer.Resolve<ITraceListener>();
            _traceListener.WriteTestOutput("Broker completing publishing to " + registeredSinks.Count + " sinks");

            var completionMessage = new ReqnrollCucumberMessage
            {
                CucumberMessageSource = cucumberMessageSource
            };

            foreach (var sink in registeredSinks)
            {
                _traceListener.WriteTestOutput($"Broker publishing completion for {cucumberMessageSource}");

                await sink.Publish(completionMessage);
            }
        }
    }
}
