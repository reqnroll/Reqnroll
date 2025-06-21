using System.Collections.Generic;

namespace Reqnroll.Formatters.Configuration;

public interface IFormattersConfigurationProvider
{
    bool Enabled { get; }
    IDictionary<string, string> GetFormatterConfigurationByName(string formatterName);
}