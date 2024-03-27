using System;
using System.Diagnostics;
using Reqnroll.TestProjectGenerator;

namespace Reqnroll.SystemTests.Drivers;

class ConsoleOutputConnector : IOutputWriter
{
    private readonly Stopwatch _stopwatch = new();
    private TimeSpan _previousElapsed = TimeSpan.Zero;

    public ConsoleOutputConnector()
    {
        _stopwatch.Start();
        WriteLine("Start");
    }

    public void WriteLine(string message)
    {
        var elapsed = _stopwatch.Elapsed;
        message = $"{elapsed:c}(+{(elapsed - _previousElapsed).TotalMilliseconds:0}ms):{message}";
        _previousElapsed = elapsed;

        Console.WriteLine(message);
    }

    public void WriteLine(string format, params object[] args)
    {
        WriteLine(string.Format(format, args));
    }
}