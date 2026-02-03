using Reqnroll.EnvironmentAccess;
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
    private readonly IVariableSubstitutionService _variableSubstitutionService;

    public bool Enabled => _resolvedConfiguration.Value.Enabled;

    public FormattersConfigurationProvider(IFileBasedConfigurationResolver fileBasedConfigurationResolver, IJsonEnvironmentConfigurationResolver jsonEnvironmentConfigurationResolver, IKeyValueEnvironmentConfigurationResolver keyValueEnvironmentConfigurationResolver, IFormattersConfigurationDisableOverrideProvider envVariableDisableFlagProvider, IVariableSubstitutionService variableSubstitutionService)
    {
        _resolvers = [fileBasedConfigurationResolver, jsonEnvironmentConfigurationResolver, keyValueEnvironmentConfigurationResolver];
        _resolvedConfiguration = new Lazy<FormattersConfiguration>(ResolveConfiguration);
        _envVariableDisableFlagProvider = envVariableDisableFlagProvider;
        _variableSubstitutionService = variableSubstitutionService;
    }

    /// <inheritdoc />
    public FormatterConfiguration GetFormatterConfiguration(string formatterName)
    {
        var config = _resolvedConfiguration.Value;
        if (config.Formatters.TryGetValue(formatterName, out var formatterConfig))
            return formatterConfig;
        return null;
    }

    /// <inheritdoc />
    [Obsolete("Use GetFormatterConfiguration instead for type-safe access to configuration values.")]
    public IDictionary<string, object> GetFormatterConfigurationByName(string formatterName)
    {
        var config = GetFormatterConfiguration(formatterName);
        return config?.ToDictionary();
    }

    private FormattersConfiguration ResolveConfiguration()
    {
        var combinedConfig = new Dictionary<string, FormatterConfiguration>(StringComparer.OrdinalIgnoreCase);

        foreach (var resolver in _resolvers)
        {
            foreach (var entry in resolver.Resolve())
            {
                if (entry.Value == null)
                {
                    // null means "disable this formatter"
                    combinedConfig.Remove(entry.Key);
                }
                else if (resolver.ShouldMergeSettings && combinedConfig.TryGetValue(entry.Key, out var existing))
                {
                    // Merge: only override settings that are explicitly set in the new config
                    existing.MergeFrom(entry.Value);
                }
                else
                {
                    // Replace: set the entire configuration
                    combinedConfig[entry.Key] = entry.Value;
                }
            }
        }
        bool enabled = combinedConfig.Count > 0 && !_envVariableDisableFlagProvider.Disabled();

        return new FormattersConfiguration
        {
            Formatters = combinedConfig,
            Enabled = enabled
        };
    }

    /// <summary>
    /// Replaces all recognized placeholders in the specified template string with their corresponding values.
    /// </summary>
    /// <param name="template">The template string containing placeholders to be resolved. Placeholders should be in the expected format for
    /// variable substitution.</param>
    /// <returns>A string with all recognized placeholders in the template replaced by their resolved values. If no placeholders
    /// are found, returns the original template string.</returns>
    public string ResolveTemplatePlaceholders(string template)
    {
        return _variableSubstitutionService.ResolveTemplatePlaceholders(template);
    }

}
