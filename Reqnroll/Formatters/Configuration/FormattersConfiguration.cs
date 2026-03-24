using System.Collections.Generic;

namespace Reqnroll.Formatters.Configuration;

public class FormattersConfiguration
{
    public bool Enabled { get; set; }
    
    /// <summary>
    /// Formatter configurations with type-safe access to known properties.
    /// </summary>
    public IDictionary<string, FormatterConfiguration> Formatters { get; set; }
}