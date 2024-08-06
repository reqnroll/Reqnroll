using Reqnroll.Plugins;
using Reqnroll.UnitTestProvider;
using System.Collections.Generic;
using System.Threading.Channels;
using System.Threading.Tasks;


namespace Reqnroll.CucumberMesssages
{
    public class CucumberMessageSinkBase : ICucumberMessageSink, IRuntimePlugin
    {
        private Channel<ReqnrollCucumberMessage> _channel = Channel.CreateUnbounded<ReqnrollCucumberMessage>();
        public async Task Publish(ReqnrollCucumberMessage message)
        {
            await _channel.Writer.WriteAsync(message);
            if (message.Envelope == null)
            {
                _channel.Writer.Complete();
            }
        }

        public async IAsyncEnumerable<ReqnrollCucumberMessage> Consume()
        {
            await foreach (var message in _channel.Reader.ReadAllAsync())
            {
                yield return message;
            }
        }

        public void Initialize(RuntimePluginEvents runtimePluginEvents, RuntimePluginParameters runtimePluginParameters, UnitTestProviderConfiguration unitTestProviderConfiguration)
        {
            runtimePluginEvents.RegisterGlobalDependencies += (sender, args) => args.ObjectContainer.RegisterInstanceAs<ICucumberMessageSink>(this);
        }

    }
}
