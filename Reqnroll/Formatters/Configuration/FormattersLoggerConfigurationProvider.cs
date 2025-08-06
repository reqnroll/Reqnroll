using Reqnroll.CommonModels;
using Reqnroll.EnvironmentAccess;
using Reqnroll.Formatters.RuntimeSupport;
using System;
using System.Collections.Generic;

namespace Reqnroll.Formatters.Configuration;

public class FormattersLoggerConfigurationProvider : IFormattersLoggerConfigurationProvider
{
    private IEnvironmentWrapper _environmentWrapper;
    private IFormatterLog _log;

    public FormattersLoggerConfigurationProvider(IEnvironmentWrapper environmentWrapper, IFormatterLog log = null)
    {
        _environmentWrapper = environmentWrapper ?? throw new ArgumentNullException(nameof(environmentWrapper));
        _log = log;
    }
    public IEnumerable<IFormattersEnvironmentOverrideConfigurationResolver> GetFormattersConfigurationResolvers()
    {
        var _formattersConfigurationResolvers = new List<IFormattersEnvironmentOverrideConfigurationResolver>();
        
        var listOfFormattersResult = _environmentWrapper.GetEnvironmentVariableNames(FormattersConfigurationConstants.REQNROLL_FORMATTERS_LOGGER_ENVIRONMENT_VARIABLE_PREFIX);
        if (listOfFormattersResult is Success<IEnumerable<string>> listOfFormattersSuccess)
        {
            foreach (var formatterName in listOfFormattersSuccess.Result)
            {
                if (string.IsNullOrWhiteSpace(formatterName))
                {
                    continue;
                }
                var resolver = new EnvironmentConfigurationResolver(_environmentWrapper, formatterName, _log);
                _formattersConfigurationResolvers.Add(resolver);
            }
        }

        return _formattersConfigurationResolvers;
    }
}
