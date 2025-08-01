using Microsoft.VisualStudio.TestPlatform.ObjectModel;
using Microsoft.VisualStudio.TestPlatform.ObjectModel.Client;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Linq;

namespace FormattersTestLogger;

[FriendlyName("formatters")]
[ExtensionUri("logger://formatterslogger")]
public class FormattersLogger : ITestLoggerWithParameters
{
    public static bool IsInitialized { get; private set; } = false;
    public static Dictionary<string, string> Parameters { get; private set; } = new();
    public static string TestRunDirectory { get; private set; } = string.Empty;

    private static string _originalEnvValue;
    private static bool _envSetByLogger = false;
    private static bool _subscribed = false;

    // Meta-keys to ignore for formatter environment variable
    private static readonly HashSet<string> MetaKeys = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
    {
        "TestRunDirectory", "TargetFramework"
    };

    private JsonObject TryParseOrFixJson(string value)
    {
        // Remove unnecessary backslashes (from shell escaping)
        string cleanedValue = value.Replace("\\", "").Trim();
        // Try to parse as JSON object
        try
        {
            var node = JsonNode.Parse(cleanedValue);
            if (node is JsonObject obj)
                return obj;
        }
        catch { }
        // Handle case: {OutputFilePath:688.ndjson} (braces but not valid JSON)
        if (cleanedValue.StartsWith("{") && cleanedValue.EndsWith("}") && !cleanedValue.Contains("\""))
        {
            var inner = cleanedValue.Substring(1, cleanedValue.Length - 2).Trim();
            cleanedValue = inner;
        }
        // Try to fix common case: OutputFilePath:688.ndjson => {"OutputFilePath":"688.ndjson"}
        var colonIdx = cleanedValue.IndexOf(":");
        if (colonIdx > 0)
        {
            var key = cleanedValue.Substring(0, colonIdx).Trim().Trim('"');
            var val = cleanedValue.Substring(colonIdx + 1).Trim().Trim('"');
            var fixedJson = $"{{\"{key}\":\"{val}\"}}";
            try
            {
                var node = JsonNode.Parse(fixedJson);
                if (node is JsonObject obj)
                    return obj;
            }
            catch { }
        }
        return null;
    }

    private void SetFormattersEnvironmentVariable(Dictionary<string, string> parameters, string testRunDirectory)
    {
        var envDict = new Dictionary<string, object>();
        string effectiveTestRunDirectory = null;
        if (parameters != null && parameters.TryGetValue("testRunDirectory", out var paramTestRunDir) && !string.IsNullOrEmpty(paramTestRunDir))
        {
            effectiveTestRunDirectory = paramTestRunDir;
        }
        else if (!string.IsNullOrEmpty(testRunDirectory))
        {
            effectiveTestRunDirectory = testRunDirectory;
        }

        if (parameters != null)
        {
            foreach (var kvp in parameters)
            {
                if (MetaKeys.Contains(kvp.Key))
                    continue;
                string value = kvp.Value;
                JsonObject obj = null;
                try
                {
                    obj = TryParseOrFixJson(value);
                    if (obj != null)
                    {
                        if (obj["outputFilePath"] is JsonNode outputPathNode)
                        {
                            string outputPath = outputPathNode.GetValue<string>();
                            if (string.IsNullOrEmpty(outputPath) || (!string.IsNullOrEmpty(outputPath) && string.IsNullOrEmpty(Path.GetDirectoryName(outputPath))))
                            {
                                if (!string.IsNullOrEmpty(effectiveTestRunDirectory) && !string.IsNullOrEmpty(outputPath))
                                {
                                    string combined = Path.Combine(effectiveTestRunDirectory, outputPath);
                                    obj["outputFilePath"] = combined;
                                }
                                else if (!string.IsNullOrEmpty(effectiveTestRunDirectory) && string.IsNullOrEmpty(outputPath))
                                {
                                    obj["outputFilePath"] = effectiveTestRunDirectory;
                                }
                            }
                        }
                        envDict[kvp.Key] = obj;
                    }
                    else
                    {
                        // If not a JsonObject, keep as string
                        envDict[kvp.Key] = value;
                    }
                }
                catch (Exception)
                {
                    // If parsing fails, keep the original value as string
                    envDict[kvp.Key] = value;
                }
            }
        }
        var json = JsonSerializer.Serialize(envDict);
        json = $"{{ \"formatters\": {json} }}"; // Wrap in "formatters" key
        Environment.SetEnvironmentVariable("REQNROLL_FORMATTERS", json);
        _envSetByLogger = true;
    }

    private void SubscribeToTestRunComplete(TestLoggerEvents events)
    {
        if (!_subscribed && events != null)
        {
            events.TestRunComplete += (s, e) =>
            {
                if (_envSetByLogger)
                {
                    Environment.SetEnvironmentVariable("REQNROLL_FORMATTERS", _originalEnvValue);
                    _envSetByLogger = false;
                }
            };
            _subscribed = true;
        }
    }

    public void Initialize(TestLoggerEvents events, Dictionary<string, string> parameters)
    {
        if (_originalEnvValue == null)
        {
            _originalEnvValue = Environment.GetEnvironmentVariable("REQNROLL_FORMATTERS");
        }
        SubscribeToTestRunComplete(events);
        IsInitialized = true;
        Parameters = parameters;
        SetFormattersEnvironmentVariable(parameters, TestRunDirectory);
    }

    public void Initialize(TestLoggerEvents events, string testRunDirectory)
    {
        if (_originalEnvValue == null)
        {
            _originalEnvValue = Environment.GetEnvironmentVariable("REQNROLL_FORMATTERS");
        }
        SubscribeToTestRunComplete(events);
        TestRunDirectory = testRunDirectory;
        IsInitialized = true;
        SetFormattersEnvironmentVariable(Parameters, testRunDirectory);
    }
}
