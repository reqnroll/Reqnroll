using System;
using System.Diagnostics;
using Reqnroll.TestProjectGenerator;
using Xunit.Abstractions;

namespace Reqnroll.SystemTests.Drivers;

class XUnitOutputConnector : IOutputWriter
{
    private readonly Stopwatch _stopwatch = new();
    private readonly ITestOutputHelper _testOutputHelper;
    private TimeSpan _previousElapsed = TimeSpan.Zero;

    public XUnitOutputConnector(ITestOutputHelper testOutputHelper)
    {
        _testOutputHelper = testOutputHelper;
        _stopwatch.Start();
        WriteLine("Start");
    }

    public void WriteLine(string message)
    {
        var elapsed = _stopwatch.Elapsed;
        message = $"{elapsed:c}(+{(elapsed - _previousElapsed).TotalMilliseconds:0}ms):{message}";
        _previousElapsed = elapsed;

        _testOutputHelper.WriteLine(message);
    }

    public void WriteLine(string format, params object[] args)
    {
        WriteLine(string.Format(format, args));
    }
}