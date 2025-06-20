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
/// then environment variable overrides are applied (<see cref="EnvironmentConfigurationResolver"/>).
/// </summary>
public class FormattersConfigurationProvider : IFormattersConfigurationProvider
{
    private readonly IList<IFormattersConfigurationResolverBase> _resolvers;
    private readonly Lazy<FormattersConfiguration> _resolvedConfiguration;
    private readonly IEnvVariableEnableFlagParser _envVariableEnableFlagParser;
    private bool _runtimeEnablementOverrideFlag = true;

    public bool Enabled => _runtimeEnablementOverrideFlag && _resolvedConfiguration.Value.Enabled;

    public FormattersConfigurationProvider(IDictionary<string, IFormattersConfigurationResolver> resolvers, IFormattersEnvironmentOverrideConfigurationResolver environmentOverrideConfigurationResolver, IEnvVariableEnableFlagParser envVariableEnableFlagParser)
    {
        var fileResolver = resolvers["fileBasedResolver"];
        _resolvers = [fileResolver, environmentOverrideConfigurationResolver];
        _resolvedConfiguration = new Lazy<FormattersConfiguration>(ResolveConfiguration);
        _envVariableEnableFlagParser = envVariableEnableFlagParser;
    }

    public IDictionary<string, string> GetFormatterConfigurationByName(string formatterName)
    {
        var config = _resolvedConfiguration.Value;
        if (config.Formatters.TryGetValue(formatterName, out var formatterConfig))
            return formatterConfig;
        return new Dictionary<string, string>();
    }

    private FormattersConfiguration ResolveConfiguration()
    {
        var combinedConfig = new Dictionary<string, IDictionary<string, string>>();

        foreach (var resolver in _resolvers)
        {
            foreach (var entry in resolver.Resolve())
            {
                combinedConfig[entry.Key] = entry.Value;
            }
        }
        bool enabled = combinedConfig.Count > 0 && _envVariableEnableFlagParser.Parse();

        return new FormattersConfiguration
        {
            Formatters = combinedConfig,
            Enabled = enabled
        };
    }
}