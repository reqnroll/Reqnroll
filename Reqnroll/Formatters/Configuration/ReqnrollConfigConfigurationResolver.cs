using Reqnroll.Analytics.UserId;
using Reqnroll.Configuration;
using Reqnroll.Formatters.RuntimeSupport;
using Reqnroll.Utils;
using System;
using System.Collections.Generic;
using System.Text.Json;

namespace Reqnroll.Formatters.Configuration;

/// <summary>
/// This class uses the Reqnroll Configuration and config loader to provide the formatters configuration.
/// </summary>
public class ReqnrollConfigConfigurationResolver : IReqnrollConfigConfigurationResolver
{
    private readonly IConfigurationLoader _configurationLoader;
    private readonly IFormatterLog _log;

    public ReqnrollConfigConfigurationResolver(
        IConfigurationLoader configurationLoader,
        IFormatterLog log = null)
    {
        _configurationLoader = configurationLoader;
        _log = log;
    }

    /// <summary>
    /// File-based configuration replaces entirely (does not merge with previous settings).
    /// </summary>
    public bool ShouldMergeSettings => false;

    public IDictionary<string, FormatterConfiguration> Resolve()
    {
        var reqnrollConfig = _configurationLoader.Load(ConfigurationLoader.GetDefault());
        return reqnrollConfig.Formatters ?? new Dictionary<string, FormatterConfiguration>(StringComparer.OrdinalIgnoreCase);
    }

}