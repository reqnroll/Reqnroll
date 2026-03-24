using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Reqnroll.Configuration.JsonConfig;

/// <summary>
/// Represents a collection of formatter configuration options, keyed by formatter name.
/// </summary>
/// <remarks>This json configuration element is used when overriding a formatter/s configuration by environment variable.
/// The json should be structured as: 
///     { "formatters": 
///         { "myformatter":
///             "settingKey": "settingValue"
///         }
///      }
/// </remarks>
public class FormattersElement
{
    [JsonPropertyName("formatters")]
    public IDictionary<string, FormatterOptionsElement> Formatters { get; set; }
}
