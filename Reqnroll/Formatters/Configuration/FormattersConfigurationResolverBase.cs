using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;

namespace Reqnroll.Formatters.Configuration;

public abstract class FormattersConfigurationResolverBase : IFormattersConfigurationResolverBase
{
    protected const string FORMATTERS_KEY = "formatters";

    public IDictionary<string, IDictionary<string, object>> Resolve()
    {
        var result = new Dictionary<string, IDictionary<string, object>>(StringComparer.OrdinalIgnoreCase);
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
            var formattersDict = GetConfigValue(formatters) as IDictionary<string, object>;
            if (formattersDict != null)
            {
                foreach (var kvp in formattersDict)
                {
                    if (kvp.Value is IDictionary<string, object> formatterConfig)
                    {
                        result.Add(kvp.Key, formatterConfig);
                    }
                }
            }
        }
    }

    private object GetConfigValue(JsonElement element, string propertyName = null)
    {
        // Check if this property is the AttachmentHandlingOptions section
        if (propertyName != null && propertyName.Equals("attachmentHandlingOptions", StringComparison.OrdinalIgnoreCase) &&
            element.ValueKind == JsonValueKind.Object)
        {
            var dict = GetConfigValue(element) as IDictionary<string, object>;
            if (dict != null)
            {
                AttachmentHandlingOption attachmentHandlingOption = AttachmentHandlingOption.None;
                string externalPath = null;
                if (dict.TryGetValue("attachmentHandling", out var handlingObj) && handlingObj is AttachmentHandlingOption)
                {
                    attachmentHandlingOption = (AttachmentHandlingOption)handlingObj;
                }
                if (dict.TryGetValue("externalAttachmentsStoragePath", out var pathObj) && pathObj is string pathStr)
                {
                    externalPath = pathStr;
                }
                return new AttachmentHandlingOptions(attachmentHandlingOption, externalPath);
            }
        }

        // Check if this property should be parsed as an enum
        if (propertyName != null && TryParseAsEnum(element, propertyName, out object enumValue))
        {
            return enumValue;
        }

        return element.ValueKind switch
        {
            JsonValueKind.Object => element.EnumerateObject()
                .ToDictionary(prop => prop.Name, prop => GetConfigValue(prop.Value, prop.Name), StringComparer.OrdinalIgnoreCase),
            JsonValueKind.Array => element.EnumerateArray()
                .Select(item => GetConfigValue(item)).ToList(),
            JsonValueKind.String => element.GetString(),
            JsonValueKind.Number => element.TryGetInt64(out long l) ? l : element.GetDouble(),
            JsonValueKind.True => true,
            JsonValueKind.False => false,
            JsonValueKind.Null => null,
            _ => element
        };
    }
    protected virtual bool TryParseAsEnum(JsonElement element, string propertyName, out object enumValue)
    {
        enumValue = null;

        if (propertyName.Equals("attachmentHandling", StringComparison.OrdinalIgnoreCase) &&
            element.ValueKind == JsonValueKind.String)
        {
            var stringValue = element.GetString();
            if (Enum.TryParse<AttachmentHandlingOption>(stringValue, out var parsed))
            {
                enumValue = parsed;
                return true;
            }
            // TODO: Handle invalid enum value case if necessary or change above parse to ignore case
        }

        // Add more enum property mappings here as needed

        return false;
    }
}