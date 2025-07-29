using Io.Cucumber.Messages.Types;
using Reqnroll.Formatters.PayloadProcessing.Cucumber;
using Reqnroll.Formatters.PubSub;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Reqnroll.Formatters.ExecutionTracking;

internal class BufferedMessagePublisher : IPublishMessage
{
    private enum BufferringState
    {
        Buffering,
        PassThru
    }
    private BufferringState _state = BufferringState.Buffering;
    private readonly List<Envelope> _bufferedMessages = new List<Envelope>();
    private readonly IPublishMessage _publisher;

    public BufferedMessagePublisher(IPublishMessage publisher)
    {
        _publisher = publisher ?? throw new ArgumentNullException(nameof(publisher));
    }
    public async Task PublishAsync(Envelope featureMessages)
    {
        if (featureMessages.Content() is TestCase)
        {
            await _publisher.PublishAsync(featureMessages);
            await FlushBuffer();
            _state = BufferringState.PassThru;
        }
        else if (_state == BufferringState.Buffering)
        {
            _bufferedMessages.Add(featureMessages);
        }
        else
        {
            await _publisher.PublishAsync(featureMessages);
        }
    }

    private async Task FlushBuffer()
    {
        foreach (var message in _bufferedMessages)
        {
            await _publisher.PublishAsync(message);
        }
        _bufferedMessages.Clear();
    }
}
