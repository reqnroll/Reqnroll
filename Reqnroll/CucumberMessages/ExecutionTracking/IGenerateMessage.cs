using Io.Cucumber.Messages.Types;
using Reqnroll.Events;
using System;
using System.Collections.Generic;
using System.Text;

namespace Reqnroll.CucumberMessages.ExecutionTracking
{
    /// <summary>
    /// This interface signifies that the implementer can generate message(s) based upon an ExecutionEvent
    /// </summary>
    public interface IGenerateMessage
    {
        public IEnumerable<Envelope> GenerateFrom(ExecutionEvent executionEvent);
    }
}
