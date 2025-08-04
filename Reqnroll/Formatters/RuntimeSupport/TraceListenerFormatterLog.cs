using Reqnroll.Tracing;
using System;
using System.Collections.Generic;

namespace Reqnroll.Formatters.RuntimeSupport;

public class TraceListenerFormatterLog(ITraceListener tl) : IFormatterLog
{
    private readonly List<string> _entries = new();
    private bool _hasDumped = false;

    public void WriteMessage(string message)
    {
        _entries.Add($"{DateTime.Now:HH:mm:ss.fff}: {message}");
    }

    public void DumpMessages()
    {
        if (!_hasDumped)
            foreach (var msg in _entries)
            {
                tl.WriteToolOutput(msg);
            }
        _hasDumped = true;
    }
}