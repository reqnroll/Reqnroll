using System;

namespace Reqnroll.RuntimeTests.AssistTests.ExampleEntities;

public abstract class AbstractMessage<TMessageContent>
{
    public DateTimeOffset MessageCreatedAt { get; protected init; }
    public string MessageContentType { get; } = nameof(TMessageContent);
    public TMessageContent MessageContent { get; protected init; }
}