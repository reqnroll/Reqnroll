using Io.Cucumber.Messages.Types;
using Reqnroll.Events;
using System;
using System.Collections.Generic;
using System.Text;

namespace Reqnroll.CucumberMessages.ExecutionTracking
{
    public interface IGenerateMessage
    {
        public IEnumerable<Envelope> GenerateFrom(ExecutionEvent executionEvent);
    }
}
