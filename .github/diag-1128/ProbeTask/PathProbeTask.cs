using System;
using System.Diagnostics;
using System.IO;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;

public class PathProbeTask : Task
{
    public string Label { get; set; }

    public override bool Execute()
    {
        var p = Process.GetCurrentProcess();
        Log.LogMessage(MessageImportance.High, $"[probe:{Label}] process: {p.MainModule.FileName} (pid {p.Id}, 64bit: {Environment.Is64BitProcess})");
        Log.LogMessage(MessageImportance.High, $"[probe:{Label}] CLR: {Environment.Version}, TargetFrameworkName: {AppDomain.CurrentDomain.SetupInformation.TargetFrameworkName ?? "<null>"}");
        Log.LogMessage(MessageImportance.High, $"[probe:{Label}] config file: {AppDomain.CurrentDomain.SetupInformation.ConfigurationFile}");
        foreach (var sw in new[] { "Switch.System.IO.UseLegacyPathHandling", "Switch.System.IO.BlockLongPaths" })
        {
            var known = AppContext.TryGetSwitch(sw, out var value);
            Log.LogMessage(MessageImportance.High, $"[probe:{Label}] {sw}: {(known ? value.ToString() : "<not set>")}");
        }

        var dir = Path.Combine(Path.GetTempPath(), "rr-path-probe", Label, Guid.NewGuid().ToString("N"));
        try
        {
            Directory.CreateDirectory(@"\\?\" + dir);
            Log.LogMessage(MessageImportance.High, $"[probe:{Label}] CreateDirectory(\\\\?\\...) OK");
        }
        catch (Exception ex)
        {
            Log.LogMessage(MessageImportance.High, $"[probe:{Label}] CreateDirectory(\\\\?\\...) FAILED: {ex.GetType().Name}: {ex.Message}");
        }
        return true;
    }
}
