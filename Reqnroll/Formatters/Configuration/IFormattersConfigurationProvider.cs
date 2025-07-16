using System.Collections.Generic;

namespace Reqnroll.Formatters.Configuration;

public interface IFormattersConfigurationProvider
{
    bool Enabled { get; }
    IDictionary<string, object> GetFormatterConfigurationByName(string formatterName);
}