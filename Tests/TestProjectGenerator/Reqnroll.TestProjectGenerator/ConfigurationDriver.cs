using System;
using System.Configuration;
using System.IO;

namespace Reqnroll.TestProjectGenerator;

public class ConfigurationDriver
{
    public string TempFolderPath => GetConfigSetting("tempFolder", Path.GetTempPath());
    public string TestProjectFolderName => GetConfigSetting("testProjectFolder", "RR");
    public string VSTestPath => GetConfigSetting("vstestPath", "Common7\\IDE\\CommonExtensions\\Microsoft\\TestWindow");
    public string MsBuildPath => GetConfigSetting(nameof(MsBuildPath));
    public bool PipelineMode => GetConfigSwitch(nameof(PipelineMode));
    public bool PerRunNuGetPackages => GetConfigSwitch(nameof(PerRunNuGetPackages));
    public string GlobalNuGetPackages => GetConfigSetting(nameof(GlobalNuGetPackages));

    public bool GetConfigSwitch(string key, bool defaultValue = false)
    {
        return GetConfigSetting(key, defaultValue.ToString()).Equals("true", StringComparison.InvariantCultureIgnoreCase);
    }

    public string GetConfigSetting(string key, string defaultValue = null)
    {
        var envSetting = Environment.GetEnvironmentVariable($"REQNROLL_TEST_{key}".ToUpperInvariant());
        if (!string.IsNullOrEmpty(envSetting)) return envSetting;

        var configSetting = ConfigurationManager.AppSettings[key];
        if (!string.IsNullOrEmpty(configSetting)) return configSetting;

        return defaultValue;
    }
}