using System;
using System.IO;

static class Program
{
    static int Main(string[] args)
    {
        var label = args.Length > 0 ? args[0] : "app";
        Console.WriteLine($"[{label}] 64bit: {Environment.Is64BitProcess}, CLR: {Environment.Version}, TFN: {AppDomain.CurrentDomain.SetupInformation.TargetFrameworkName ?? "<null>"}");
        Console.WriteLine($"[{label}] config: {AppDomain.CurrentDomain.SetupInformation.ConfigurationFile}");
        foreach (var sw in new[] { "Switch.System.IO.UseLegacyPathHandling", "Switch.System.IO.BlockLongPaths" })
            Console.WriteLine($"[{label}] {sw}: {(AppContext.TryGetSwitch(sw, out var v) ? v.ToString() : "<not set>")}");
        try
        {
            Directory.CreateDirectory(@"\\?\" + Path.Combine(Path.GetTempPath(), "rr-path-probe", label, Guid.NewGuid().ToString("N")));
            Console.WriteLine($@"[{label}] CreateDirectory(\\?\...) OK");
        }
        catch (Exception ex)
        {
            Console.WriteLine($@"[{label}] CreateDirectory(\\?\...) FAILED: {ex.GetType().Name}: {ex.Message}");
        }
        return 0;
    }
}
