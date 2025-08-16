using System;
using System.Collections.Generic;

namespace Reqnroll.Formatters.Configuration;

/// <summary>
/// This class is responsible for determining the configuration of the Cucumber Messages subsystem.
/// It is wired into the object container as a singleton and is a dependency of the PubSub classes.
/// 
/// When any consumer of this class asks for one of the properties of <see cref="IFormattersConfigurationProvider"/>,
/// the class will resolve the configuration (only once).
/// 
/// One or more profiles may be read from the configuration file (<see cref="FileBasedConfigurationResolver"/>)
/// then environment variable overrides are applied (first <see cref="JsonEnvironmentConfigurationResolver"/>, then <see cref="KeyValueEnvironmentConfigurationResolver"/>).
/// </summary>
public class FormattersConfigurationProvider : IFormattersConfigurationProvider
{
    private readonly IList<IFormattersConfigurationResolverBase> _resolvers;
    private readonly Lazy<FormattersConfiguration> _resolvedConfiguration;
    private readonly IFormattersConfigurationDisableOverrideProvider _envVariableDisableFlagProvider;
    public bool Enabled => _resolvedConfiguration.Value.Enabled;

    public FormattersConfigurationProvider(IFileBasedConfigurationResolver fileBasedConfigurationResolver, IJsonEnvironmentConfigurationResolver jsonEnvironmentConfigurationResolver, IKeyValueEnvironmentConfigurationResolver keyValueEnvironmentConfigurationResolver, IFormattersConfigurationDisableOverrideProvider envVariableDisableFlagProvider)
    {
        _resolvers = [fileBasedConfigurationResolver, jsonEnvironmentConfigurationResolver, keyValueEnvironmentConfigurationResolver];
        _resolvedConfiguration = new Lazy<FormattersConfiguration>(ResolveConfiguration);
        _envVariableDisableFlagProvider = envVariableDisableFlagProvider;
    }

    public IDictionary<string, object> GetFormatterConfigurationByName(string formatterName)
    {
        var config = _resolvedConfiguration.Value;
        if (config.Formatters.TryGetValue(formatterName, out var formatterConfig))
            return formatterConfig;
        return null;
    }

    private FormattersConfiguration ResolveConfiguration()
    {
        var combinedConfig = new Dictionary<string, IDictionary<string, object>>(StringComparer.OrdinalIgnoreCase);

        foreach (var resolver in _resolvers)
        {
            foreach (var entry in resolver.Resolve())
            {
                if (entry.Value == null) 
                    combinedConfig.Remove(entry.Key);
                else 
                    combinedConfig[entry.Key] = entry.Value;
            }
        }
        bool enabled = combinedConfig.Count > 0 && !_envVariableDisableFlagProvider.Disabled();

        return new FormattersConfiguration
        {
            Formatters = combinedConfig,
            Enabled = enabled
        };
    }
}
