using Reqnroll.CommonModels;
using Reqnroll.EnvironmentAccess;
using Reqnroll.Formatters.RuntimeSupport;
using System;
using System.Text.Json;

namespace Reqnroll.Formatters.Configuration;

public class JsonEnvironmentConfigurationResolver : FormattersConfigurationResolverBase, IJsonEnvironmentConfigurationResolver
{
    private readonly IEnvironmentWrapper _environmentWrapper;
    private readonly IFormatterLog _log;
    private readonly string _environmentVariableName; 

    public JsonEnvironmentConfigurationResolver(
        IEnvironmentWrapper environmentWrapper,
        IFormatterLog log = null)
    {
        _environmentWrapper = environmentWrapper;
        _log = log;
        _environmentVariableName = FormattersConfigurationConstants.REQNROLL_FORMATTERS_ENVIRONMENT_VARIABLE;
    }

    internal JsonEnvironmentConfigurationResolver(
        IEnvironmentWrapper environmentWrapper,
        string environmentVariableName,
        IFormatterLog log = null)
    {
        _environmentWrapper = environmentWrapper ?? throw new ArgumentNullException(nameof(environmentWrapper));
        _log = log;
        _environmentVariableName = environmentVariableName ?? throw new ArgumentNullException(nameof(environmentVariableName));
    }

    protected override JsonDocument GetJsonDocument()
    {
        try
        {
            var formatters = _environmentWrapper.GetEnvironmentVariable(_environmentVariableName);

            if (formatters is Success<string> formattersSuccess)
            {
                if (string.IsNullOrWhiteSpace(formattersSuccess.Result))
                {
                    _log?.WriteMessage($"Environment variable {_environmentVariableName} is empty");
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
                    _log?.WriteMessage($"Failed to parse JSON from environment variable {_environmentVariableName}: {ex.Message}");
                }
            }
            else if (formatters is Failure<string> failure)
            {
                _log?.WriteMessage($"Environment variable {_environmentVariableName} not applied: {failure.Description}");
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