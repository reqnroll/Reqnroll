using System;

namespace Reqnroll.Formatters.PubSub
{
    public interface INotifyPublisherReady
    {
        event EventHandler<PublisherReadyEventArgs> Initialized;
        bool IsInitialized { get; }
    }
}