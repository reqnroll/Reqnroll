using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace Reqnroll.Formatters.Configuration;

public interface IFormattersConfigurationProvider
{
    bool Enabled { get; }

    /// <summary>
    /// Gets the typed configuration for a formatter by name.
    /// </summary>
    /// <param name="formatterName">The name of the formatter.</param>
    /// <returns>The FormatterConfiguration, or null if the formatter is not configured.</returns>
    FormatterConfiguration GetFormatterConfiguration(string formatterName);

    string ResolveTemplatePlaceholders(string template);
}