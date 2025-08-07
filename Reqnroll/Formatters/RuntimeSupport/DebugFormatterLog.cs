using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Reqnroll.Formatters.RuntimeSupport;

public class DebugFormatterLog : IFormatterLog
{
    private readonly List<string> _entries = new();
    private bool _hasDumped = false;

    public void WriteMessage(string message)
    {
#if DEBUG
        _entries.Add($"{DateTime.Now:HH:mm:ss.fff}: {message}");
#endif
    }

    public void DumpMessages()
    {
        if (!_hasDumped)
            foreach (var msg in _entries)
            {
                Debug.WriteLine(msg);
            }
        _hasDumped = true;
    }
}
