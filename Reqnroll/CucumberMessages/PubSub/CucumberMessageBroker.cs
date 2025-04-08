﻿using Io.Cucumber.Messages.Types;
using Reqnroll.BoDi;
using Reqnroll.CucumberMessages.PayloadProcessing.Cucumber;
using Reqnroll.Tracing;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Security.Authentication.ExtendedProtection;
using System.Text;
using System.Threading.Tasks;


namespace Reqnroll.CucumberMessages.PubSub
{

    public interface ICucumberMessageBroker
    {
        bool Enabled { get; }
        Task PublishAsync(Envelope featureMessages);
    }

    /// <summary>
    /// Cucumber Message implementation is a simple Pub/Sub implementation.
    /// This broker mediates between the (singleton) CucumberMessagePublisher and (one or more) CucumberMessageSinks
    /// 
    /// The pub/sub mechanism is considered to be turned "OFF" if no sinks are registered
    /// </summary>
    public class CucumberMessageBroker : ICucumberMessageBroker
    {
        private IObjectContainer _objectContainer;

        public bool Enabled => RegisteredSinks.Value.ToList().Count > 0;

        private Lazy<IEnumerable<ICucumberMessageSink>> RegisteredSinks;

        public CucumberMessageBroker(IObjectContainer objectContainer)
        {
            _objectContainer = objectContainer;
            RegisteredSinks = new Lazy<IEnumerable<ICucumberMessageSink>>(() => _objectContainer.ResolveAll<ICucumberMessageSink>());
        }
        public async Task PublishAsync(Envelope message)
        {
            foreach (var sink in RegisteredSinks.Value)
            {
                await sink.PublishAsync(message);
            }
        }

    }
}
