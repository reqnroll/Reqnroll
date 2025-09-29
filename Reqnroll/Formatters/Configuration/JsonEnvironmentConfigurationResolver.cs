using Reqnroll.CommonModels;
using Reqnroll.EnvironmentAccess;
using Reqnroll.Formatters.RuntimeSupport;
using System;
using System.Text.Json;

namespace Reqnroll.Formatters.Configuration;

public class JsonEnvironmentConfigurationResolver : FormattersConfigurationResolverBase, IJsonEnvironmentConfigurationResolver
{
    private readonly IEnvironmentOptions _environmentOptions;
    private readonly IFormatterLog _log;

    public JsonEnvironmentConfigurationResolver(
        IEnvironmentOptions environmentOptions,
        IFormatterLog log = null)
    {
        _environmentOptions = environmentOptions;
        _log = log;
    }

    protected override JsonDocument GetJsonDocument()
    {
        try
        {
            var formattersJson = _environmentOptions.FormattersJson;

            if (!string.IsNullOrWhiteSpace(formattersJson))
            {
                try
                {
                    return JsonDocument.Parse(formattersJson, new JsonDocumentOptions
                    {
                        CommentHandling = JsonCommentHandling.Skip,
                        AllowTrailingCommas = true // More lenient parsing
                    });
                }
                catch (JsonException ex)
                {
                    _log?.WriteMessage($"Failed to parse JSON from environment variable {EnvironmentOptions.REQNROLL_FORMATTERS_ENVIRONMENT_VARIABLE}: {ex.Message}");
                }
            }
            else
            {
                _log?.WriteMessage($"Environment variable {EnvironmentOptions.REQNROLL_FORMATTERS_ENVIRONMENT_VARIABLE} is not set");
            }
        }
        catch (Exception ex) when (ex is not JsonException)
        {
            // Catch any unexpected exceptions but don't let them propagate
            _log?.WriteMessage($"Unexpected error retrieving environment configuration: {ex.Message}");
        }

        return null;
    }
}