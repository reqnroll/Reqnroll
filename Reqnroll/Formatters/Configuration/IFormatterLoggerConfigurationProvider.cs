using System.Collections.Generic;

namespace Reqnroll.Formatters.Configuration;

/// <summary>
/// Implementation of this interface provides a set of IFormattersConfigurationResolver instances each of which
/// is responsible for resolving the configuration of one formatter that has been configured via the --logger mechanism.
/// </summary>
public interface IFormattersLoggerConfigurationProvider
{
    IEnumerable<IFormattersEnvironmentOverrideConfigurationResolver> GetFormattersConfigurationResolvers();
}
