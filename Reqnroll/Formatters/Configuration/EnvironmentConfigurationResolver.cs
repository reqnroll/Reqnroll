using Reqnroll.CommonModels;
using Reqnroll.EnvironmentAccess;
using Reqnroll.Formatters.RuntimeSupport;
using System;
using System.Text.Json;

namespace Reqnroll.Formatters.Configuration;

public class EnvironmentConfigurationResolver : FormattersConfigurationResolverBase, IFormattersEnvironmentOverrideConfigurationResolver
{
    private readonly IEnvironmentWrapper _environmentWrapper;
    private readonly IFormatterLog _log;

    public EnvironmentConfigurationResolver(
        IEnvironmentWrapper environmentWrapper,
        IFormatterLog? log = null)
    {
        _environmentWrapper = environmentWrapper;
        _log = log;
    }

    protected override JsonDocument GetJsonDocument()
    {
        try
        {
            var formatters = _environmentWrapper.GetEnvironmentVariable(FormattersConfigurationConstants.REQNROLL_FORMATTERS_ENVIRONMENT_VARIABLE);

            if (formatters is Success<string> formattersSuccess)
            {
                if (string.IsNullOrWhiteSpace(formattersSuccess.Result))
                {
                    _log?.WriteMessage($"Environment variable {FormattersConfigurationConstants.REQNROLL_FORMATTERS_ENVIRONMENT_VARIABLE} is empty");
                    return null;
                }

                try
                {
                    return JsonDocument.Parse(formattersSuccess.Result, new JsonDocumentOptions
                    {
                        CommentHandling = JsonCommentHandling.Skip,
                        AllowTrailingCommas = true // More lenient parsing
                    });
                }
                catch (JsonException ex)
                {
                    _log?.WriteMessage($"Failed to parse JSON from environment variable {FormattersConfigurationConstants.REQNROLL_FORMATTERS_ENVIRONMENT_VARIABLE}: {ex.Message}");
                }
            }
            else if (formatters is Failure<string> failure)
            {
                _log?.WriteMessage($"Could not retrieve environment variable {FormattersConfigurationConstants.REQNROLL_FORMATTERS_ENVIRONMENT_VARIABLE}: {failure.Description}");
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