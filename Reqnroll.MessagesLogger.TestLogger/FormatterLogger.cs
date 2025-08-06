using Microsoft.VisualStudio.TestPlatform.ObjectModel;
using Microsoft.VisualStudio.TestPlatform.ObjectModel.Client;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Linq;

namespace FormattersTestLogger;

[FriendlyName("formatter")]
[ExtensionUri("logger://formatterlogger")]
public class FormatterLogger : ITestLoggerWithParameters
{
    public bool IsInitialized { get; private set; } = false;
    public Dictionary<string, string> Parameters { get; private set; } = new();
    public string TestRunDirectory { get; private set; } = string.Empty;

    private string _originalEnvValue;
    private bool _envSetByLogger = false;
    private bool _subscribed = false;
    private string _formatterName = null;

    // Meta-keys to ignore for formatter environment variable
    private static readonly HashSet<string> MetaKeys = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
    {
        "TestRunDirectory", "TargetFramework"
    };

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
            var formatterName = parameters.ContainsKey("name") ? parameters["name"] : null;
            var outputFilePath = parameters.ContainsKey("outputFilePath") ? parameters["outputFilePath"] : null;
            var alternateFilePath = parameters.ContainsKey("LogFileName") ? parameters["LogFileName"] : null;
            outputFilePath ??= alternateFilePath;

            if (string.IsNullOrEmpty(formatterName))
            {
                return; // No formatter name provided, nothing to do
            }
            if (!string.IsNullOrEmpty(effectiveTestRunDirectory) && !string.IsNullOrEmpty(outputFilePath))
            {
                outputFilePath = Path.Combine(effectiveTestRunDirectory, outputFilePath);
            }
            else if (!string.IsNullOrEmpty(effectiveTestRunDirectory) && string.IsNullOrEmpty(outputFilePath))
            {
                outputFilePath = effectiveTestRunDirectory;
            }
            
            // Use proper JSON serialization to handle escaping correctly
            var configObject = new
            {
                formatters = new Dictionary<string, object>
                {
                    [formatterName] = new
                    {
                        outputFilePath = outputFilePath
                    }
                }
            };
            
            var json = JsonSerializer.Serialize(configObject);
            
            if (_originalEnvValue == null)
            {
                _originalEnvValue = Environment.GetEnvironmentVariable($"REQNROLL_FORMATTERS_LOGGER_{formatterName}");
            }
            Environment.SetEnvironmentVariable($"REQNROLL_FORMATTERS_LOGGER_{formatterName}", json);
            _envSetByLogger = true;
            _formatterName = formatterName;
        }
    }

    private void SubscribeToTestRunComplete(TestLoggerEvents events)
    {
        if (!_subscribed && events != null)
        {
            events.TestRunComplete += (s, e) =>
            {
                if (_envSetByLogger)
                {
                    Environment.SetEnvironmentVariable($"REQNROLL_FORMATTERS_LOGGER_{_formatterName}", _originalEnvValue);
                    _envSetByLogger = false;
                }
            };
            _subscribed = true;
        }
    }

    public void Initialize(TestLoggerEvents events, Dictionary<string, string> parameters)
    {
        SubscribeToTestRunComplete(events);
        IsInitialized = true;
        Parameters = parameters;
        SetFormattersEnvironmentVariable(parameters, TestRunDirectory);
    }

    public void Initialize(TestLoggerEvents events, string testRunDirectory)
    {
        SubscribeToTestRunComplete(events);
        TestRunDirectory = testRunDirectory;
        IsInitialized = true;
        SetFormattersEnvironmentVariable(Parameters, testRunDirectory);
    }
}
