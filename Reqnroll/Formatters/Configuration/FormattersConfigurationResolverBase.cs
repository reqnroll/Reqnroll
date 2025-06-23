using System.Collections.Generic;
using System.Text.Json;

namespace Reqnroll.Formatters.Configuration;

public abstract class FormattersConfigurationResolverBase : IFormattersConfigurationResolverBase
{
    protected const string FORMATTERS_KEY = "formatters";

    public IDictionary<string, IDictionary<string, object>> Resolve()
    {
        var result = new Dictionary<string, IDictionary<string, object>>();
        JsonDocument jsonDocument = GetJsonDocument();
        
        if (jsonDocument != null)
        {
            ProcessJsonDocument(jsonDocument, result);
        }
        
        return result;
    }
    
    protected abstract JsonDocument GetJsonDocument();
    
    protected virtual void ProcessJsonDocument(JsonDocument jsonDocument, Dictionary<string, IDictionary<string, object>> result)
    {
        if (jsonDocument.RootElement.TryGetProperty(FORMATTERS_KEY, out JsonElement formatters))
        {
            foreach(JsonProperty formatterProperty in formatters.EnumerateObject())
            {
                var configValues = new Dictionary<string, object>();
                
                if (formatterProperty.Value.ValueKind == JsonValueKind.Object)
                {
                    foreach (JsonProperty configProperty in formatterProperty.Value.EnumerateObject())
                    {
                            configValues.Add(configProperty.Name, configProperty.Value);
                    }
                }
                
                result.Add(formatterProperty.Name, configValues);
            }
        }
    }
}