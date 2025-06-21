using System.Collections.Generic;

namespace Reqnroll.Formatters.Configuration;

public class FormattersConfiguration
{
    public bool Enabled { get; set; }
    public IDictionary<string, IDictionary<string, string>> Formatters { get; set; }
}