using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using Microsoft.VisualStudio.TestPlatform.ObjectModel.Client;

namespace Reqnroll.FormatterTestLogger;

public abstract class FormatterLoggerBase : ITestLoggerWithParameters
{
    public bool IsInitialized { get; private set; }
    public Dictionary<string, string> Parameters { get; private set; } = new();
    public string TestRunDirectory { get; private set; } = string.Empty;

    private string _originalEnvValue;
    private bool _envSetByLogger;
    private bool _subscribed;
    protected string FormatterName = null;

    // Meta-keys to ignore for formatter environment variable
    // ReSharper disable once UnusedMember.Global
    protected static readonly HashSet<string> MetaKeys = new(StringComparer.OrdinalIgnoreCase)
    {
        "TestRunDirectory", "TargetFramework"
    };

    private void SetFormattersEnvironmentVariable(Dictionary<string, string> parameters, string testRunDirectory)
    {
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
            var outputFilePath = parameters.TryGetValue("outputFilePath", out string outputFilePathParameter) ? outputFilePathParameter : null;
            var alternateFilePath = parameters.TryGetValue("LogFileName", out string alternateFilePathParameter) ? alternateFilePathParameter : null;
            outputFilePath ??= alternateFilePath;

            if (string.IsNullOrEmpty(FormatterName))
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
                    [FormatterName] = new
                    {
                        outputFilePath
                    }
                }
            };
            
            var json = JsonSerializer.Serialize(configObject);
            
            if (_originalEnvValue == null)
            {
                _originalEnvValue = Environment.GetEnvironmentVariable($"REQNROLL_FORMATTERS_LOGGER_{FormatterName}");
            }
            Environment.SetEnvironmentVariable($"REQNROLL_FORMATTERS_LOGGER_{FormatterName}", json);
            _envSetByLogger = true;
        }
    }

    private void SubscribeToTestRunComplete(TestLoggerEvents events)
    {
        if (!_subscribed && events != null)
        {
            events.TestRunComplete += (_, _) =>
            {
                if (_envSetByLogger)
                {
                    Environment.SetEnvironmentVariable($"REQNROLL_FORMATTERS_LOGGER_{FormatterName}", _originalEnvValue);
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
