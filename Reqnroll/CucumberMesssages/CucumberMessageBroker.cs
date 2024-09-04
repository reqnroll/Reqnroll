using Reqnroll.BoDi;
using Reqnroll.Tracing;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Security.Authentication.ExtendedProtection;
using System.Text;
using System.Threading.Tasks;


namespace Reqnroll.CucumberMesssages
{

    public interface ICucumberMessageBroker
    {
        bool Enabled { get; }
        void Complete(string cucumberMessageSource);
        void Publish(ReqnrollCucumberMessage message);
    }

    public class CucumberMessageBroker : ICucumberMessageBroker
    {
        private IObjectContainer _objectContainer;

        public bool Enabled => _objectContainer.ResolveAll<ICucumberMessageSink>().ToList().Count > 0;

        //private ITraceListener _traceListener;

        public CucumberMessageBroker(IObjectContainer objectContainer)
        { 
            _objectContainer = objectContainer;
        }
        public void Publish(ReqnrollCucumberMessage message)
        {
            var _traceListener = _objectContainer.Resolve<ITraceListener>();

            //TODO: find a way to populate this list a single time
            var registeredSinks = _objectContainer.ResolveAll<ICucumberMessageSink>().ToList();

            foreach (var sink in registeredSinks)
            {   
                _traceListener.WriteTestOutput($"Broker publishing {message.CucumberMessageSource}");

                sink.Publish(message);
            }
        }

        // using an empty CucumberMessage to indicate completion
        public  void Complete(string cucumberMessageSource)
        {
            var registeredSinks = _objectContainer.ResolveAll<ICucumberMessageSink>().ToList();

            var _traceListener = _objectContainer.Resolve<ITraceListener>();

            var completionMessage = new ReqnrollCucumberMessage
            {
                CucumberMessageSource = cucumberMessageSource
            };

            foreach (var sink in registeredSinks)
            {
                _traceListener.WriteTestOutput($"Broker publishing completion for {cucumberMessageSource}");

                sink.Publish(completionMessage);
            }
        }
    }
}
