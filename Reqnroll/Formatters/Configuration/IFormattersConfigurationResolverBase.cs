using System.Collections.Generic;

namespace Reqnroll.Formatters.Configuration;

public interface IFormattersConfigurationResolverBase
{
    IDictionary<string, IDictionary<string, object>> Resolve();
}