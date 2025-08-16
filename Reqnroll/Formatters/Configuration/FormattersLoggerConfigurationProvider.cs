using Reqnroll.CommonModels;
using Reqnroll.EnvironmentAccess;
using Reqnroll.Formatters.RuntimeSupport;
using System;
using System.Collections.Generic;

namespace Reqnroll.Formatters.Configuration;

public class FormattersLoggerConfigurationProvider : IFormattersLoggerConfigurationProvider
{
    private readonly IEnvironmentWrapper _environmentWrapper;
    private readonly IFormatterLog _log;

    public FormattersLoggerConfigurationProvider(IEnvironmentWrapper environmentWrapper, IFormatterLog log = null)
    {
        _environmentWrapper = environmentWrapper ?? throw new ArgumentNullException(nameof(environmentWrapper));
        _log = log;
    }

    public IEnumerable<IFormattersEnvironmentOverrideConfigurationResolver> GetFormattersConfigurationResolvers()
    {
        var formattersConfigurationResolvers = new List<IFormattersEnvironmentOverrideConfigurationResolver>();

        var listOfFormattersResult = _environmentWrapper.GetEnvironmentVariables(FormattersConfigurationConstants.REQNROLL_FORMATTERS_LOGGER_ENVIRONMENT_VARIABLE_PREFIX);
        foreach (var formatterEnvironmentVariable in listOfFormattersResult)
        {
            var resolver = new EnvironmentConfigurationResolver(_environmentWrapper, formatterEnvironmentVariable.Key, _log);
            formattersConfigurationResolvers.Add(resolver);
        }

        return formattersConfigurationResolvers;
    }
}
