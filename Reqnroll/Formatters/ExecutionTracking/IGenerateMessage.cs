using Io.Cucumber.Messages.Types;
using Reqnroll.Events;
using System.Collections.Generic;

namespace Reqnroll.Formatters.ExecutionTracking;

/// <summary>
/// This interface signifies that the implementer can generate message(s) based upon an ExecutionEvent
/// </summary>
public interface IGenerateMessage
{
    public IEnumerable<Envelope> GenerateFrom(ExecutionEvent executionEvent);
}