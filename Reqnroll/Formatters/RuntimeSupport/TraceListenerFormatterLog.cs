using Reqnroll.Tracing;
using System;
using System.Collections.Generic;

namespace Reqnroll.Formatters.RuntimeSupport;

#pragma warning disable CS9113 // Parameter is unread.
public class TraceListenerFormatterLog(ITraceListener tl) : IFormatterLog
#pragma warning restore CS9113 // Parameter is unread.
{
    private readonly List<string> _entries = new();
    private bool _hasDumped = false;

    public void WriteMessage(string message)
    {
        _entries.Add($"{DateTime.Now:HH:mm:ss.fff}: {message}");
        //tl.WriteToolOutput($"{DateTime.Now:HH:mm:ss.fff}: {message}");
        //Console.WriteLine(message);
        //Debug.WriteLine(message);
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