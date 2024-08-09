using Reqnroll.BoDi;
using Reqnroll.Events;
using Reqnroll.Plugins;
using Reqnroll.Tracing;
using Reqnroll.UnitTestProvider;
using System.Collections.Generic;
using System.Threading.Channels;
using System.Threading.Tasks;


namespace Reqnroll.CucumberMesssages
{
    public class CucumberMessageSinkBase 
    {
        protected IObjectContainer _testThreadContainer;

        private ICucumberMessagePublisher _publisher;
        private Channel<ReqnrollCucumberMessage> _channel = Channel.CreateUnbounded<ReqnrollCucumberMessage>();

        public void Initialize(RuntimePluginEvents runtimePluginEvents, RuntimePluginParameters runtimePluginParameters, UnitTestProviderConfiguration unitTestProviderConfiguration)
        {
            runtimePluginEvents.CustomizeTestThreadDependencies += (sender, args) =>
            {
                _testThreadContainer = args.ObjectContainer;
                _publisher = args.ObjectContainer.Resolve<ICucumberMessagePublisher>();
                var testThreadExecutionEventPublisher = args.ObjectContainer.Resolve<ITestThreadExecutionEventPublisher>();
                _publisher.HookIntoTestThreadExecutionEventPublisher(testThreadExecutionEventPublisher);
            };
        }

        public async Task Publish(ReqnrollCucumberMessage message)
        {
            var traceListener = _testThreadContainer.Resolve<ITraceListener>();
            traceListener.WriteTestOutput($"Cucumber Message Sink publishing message.");

            await _channel.Writer.WriteAsync(message);
            if (message.Envelope == null)
            {
                _channel.Writer.Complete();
            }
        }

        public async IAsyncEnumerable<ReqnrollCucumberMessage> Consume()
        {
            var traceListener = _testThreadContainer.Resolve<ITraceListener>();
            traceListener.WriteTestOutput($"Cucumber Message Sink Consume() called.");

            await foreach (var message in _channel.Reader.ReadAllAsync())
            {
                //        _traceListener.WriteTestOutput($"Cucumber Message Sink consuming message.");
                yield return message;
            }
        }
    }
}
