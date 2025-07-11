﻿using Io.Cucumber.Messages.Types;
using Reqnroll.Formatters.PubSub;
using System.Threading.Tasks;

namespace Reqnroll.Formatters;

public interface ICucumberMessageFormatter
{
    void LaunchSink(ICucumberMessageBroker broker);
    string Name { get; }
    Task PublishAsync(Envelope message);
}