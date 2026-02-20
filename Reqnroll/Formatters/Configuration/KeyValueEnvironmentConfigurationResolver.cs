using System;
using Reqnroll.EnvironmentAccess;
using Reqnroll.Formatters.RuntimeSupport;
using System.Collections.Generic;

namespace Reqnroll.Formatters.Configuration;

internal class KeyValueEnvironmentConfigurationResolver(IEnvironmentOptions environmentOptions, IFormatterLog log = null) : IKeyValueEnvironmentConfigurationResolver
{
    private readonly IEnvironmentOptions _environmentWrapper = environmentOptions ?? throw new ArgumentNullException(nameof(environmentOptions));

    /// <summary>
    /// KeyValue-based configuration merges with existing settings (only overrides specified values).
    /// </summary>
    public bool ShouldMergeSettings => true;

    public IDictionary<string, FormatterConfiguration> Resolve()
    {
        var result = new Dictionary<string, FormatterConfiguration>(StringComparer.OrdinalIgnoreCase);

        var environmentVariables = _environmentWrapper.FormatterSettings;
        foreach (var formatterEnvironmentVariable in environmentVariables)
        {
            var formatterName = formatterEnvironmentVariable.Key;
            var formatterConfiguration = formatterEnvironmentVariable.Value?.Trim();
            if (string.IsNullOrEmpty(formatterConfiguration))
            {
                continue;
            }

            log?.WriteMessage($"Configuring formatter '{formatterName}' via environment variable {formatterEnvironmentVariable.Key}={formatterEnvironmentVariable.Value}");

            if (formatterConfiguration.Equals("true", StringComparison.InvariantCultureIgnoreCase))
            {
                result[formatterName] = new FormatterConfiguration();
                continue;
            }

            if (formatterConfiguration.Equals("false", StringComparison.InvariantCultureIgnoreCase))
            {
                result[formatterName] = null;
                continue;
            }

            var settings = formatterConfiguration.Split(';');

            var config = new FormatterConfiguration();
            foreach (string setting in settings)
            {
                var keyValue = setting.Split(['='], 2);
                if (keyValue.Length == 1)
                    throw new ReqnrollException($"Could not parse setting '{setting}' for formatter '{formatterName}' when processing the environment variable {formatterEnvironmentVariable.Key}. Please use semicolon separated list of 'key=value' settings or 'true'.");

                var key = keyValue[0].Trim();
                var value = keyValue[1].Trim();

                if (string.Equals(key, "outputFilePath", StringComparison.OrdinalIgnoreCase))
                {
                    config.OutputFilePath = value;
                }
                else
                {
                    config.AdditionalSettings[key] = value;
                }
            }

            result[formatterName] = config;
        }

        return result;
    }
}