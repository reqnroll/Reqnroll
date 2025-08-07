using Io.Cucumber.Messages.Types;
using Reqnroll.Formatters.PayloadProcessing.Cucumber;
using Reqnroll.Formatters.PubSub;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Reqnroll.Formatters.ExecutionTracking;

internal class BufferedMessagePublisher : IMessagePublisher
{
    private enum BufferingState
    {
        Buffering,
        PassThru
    }
    private BufferingState _state = BufferingState.Buffering;
    private readonly List<Envelope> _bufferedMessages = new();
    private readonly IMessagePublisher _publisher;

    public BufferedMessagePublisher(IMessagePublisher publisher)
    {
        _publisher = publisher ?? throw new ArgumentNullException(nameof(publisher));
    }

    public async Task PublishAsync(Envelope message)
    {
        if (message.Content() is TestCase)
        {
            await _publisher.PublishAsync(message);
            await FlushBuffer();
            _state = BufferingState.PassThru;
        }
        else if (_state == BufferingState.Buffering)
        {
            _bufferedMessages.Add(message);
        }
        else
        {
            await _publisher.PublishAsync(message);
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
