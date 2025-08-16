using System;
using Reqnroll.EnvironmentAccess;
using Reqnroll.Formatters.RuntimeSupport;
using System.Collections.Generic;

namespace Reqnroll.Formatters.Configuration;

internal class KeyValueEnvironmentConfigurationResolver(IEnvironmentWrapper environmentWrapper, IFormatterLog log = null) : IKeyValueEnvironmentConfigurationResolver
{
    private readonly IEnvironmentWrapper _environmentWrapper = environmentWrapper ?? throw new ArgumentNullException(nameof(environmentWrapper));

    public IDictionary<string, IDictionary<string, object>> Resolve()
    {
        var result = new Dictionary<string, IDictionary<string, object>>(StringComparer.InvariantCultureIgnoreCase);

        var environmentVariables = _environmentWrapper.GetEnvironmentVariables(FormattersConfigurationConstants.REQNROLL_FORMATTERS_ENVIRONMENT_VARIABLE_PREFIX);
        foreach (var formatterEnvironmentVariable in environmentVariables)
        {
            var formatterName = formatterEnvironmentVariable.Key.Substring(FormattersConfigurationConstants.REQNROLL_FORMATTERS_ENVIRONMENT_VARIABLE_PREFIX.Length);
            var formatterConfiguration = formatterEnvironmentVariable.Value?.Trim();
            if (string.IsNullOrEmpty(formatterConfiguration))
                continue;

            log?.WriteMessage($"Configuring formatter '{formatterName}' via environment variable {formatterEnvironmentVariable.Key}={formatterEnvironmentVariable.Value}");

            if (formatterConfiguration.Equals("true", StringComparison.InvariantCultureIgnoreCase))
            {
                result[formatterName] = new Dictionary<string, object>();
                continue;
            }

            if (formatterConfiguration.Equals("false", StringComparison.InvariantCultureIgnoreCase))
            {
                result[formatterName] = null;
                continue;
            }

            var settings = formatterConfiguration.Split(';');

            var configValues = new Dictionary<string, object>();
            foreach (string setting in settings)
            {
                var keyValue = setting.Split(['='], 2);
                if (keyValue.Length == 1)
                    throw new ReqnrollException($"Could not parse setting '{setting}' for formatter '{formatterName}' when processing the environment variable {formatterEnvironmentVariable.Key}. Please use semicolon separated list of 'key=value' settings or 'true'.");

                configValues[keyValue[0].Trim()] = keyValue[1].Trim();
            }

            result[formatterName] = configValues;
        }

        return result;
    }
}