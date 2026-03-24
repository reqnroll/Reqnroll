using System.Collections.Generic;

namespace Reqnroll.Formatters.Configuration;

public interface IFormattersConfigurationResolverBase
{
    IDictionary<string, FormatterConfiguration> Resolve();
    
    /// <summary>
    /// Indicates whether this resolver's settings should be merged with existing settings (true)
    /// or should completely replace them (false).
    /// KeyValue-based resolvers typically merge individual settings, while JSON-based resolvers replace entirely.
    /// </summary>
    bool ShouldMergeSettings { get; }
}