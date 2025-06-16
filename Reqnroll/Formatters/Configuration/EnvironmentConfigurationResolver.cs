using Reqnroll.CommonModels;
using Reqnroll.EnvironmentAccess;
using System.Collections.Generic;
using System.Text.Json;

namespace Reqnroll.Formatters.Configuration
{
    public class EnvironmentConfigurationResolver : IFormattersEnvironmentOverrideConfigurationResolver
    {
        private readonly IEnvironmentWrapper _environmentWrapper;

        public EnvironmentConfigurationResolver(IEnvironmentWrapper environmentWrapper)
        {
            _environmentWrapper = environmentWrapper;
        }

        public IDictionary<string, string> Resolve()
        {
            // Logic for environment variable overrides
            var result = new Dictionary<string, string>();
            var formatters = _environmentWrapper.GetEnvironmentVariable(FormattersConfigurationConstants.REQNROLL_FORMATTERS_ENVIRONMENT_VARIABLE);
            // treating formatters as a json string containing an object, iterate over the properties and add them to the configuration, replacing all existing values;
            if (formatters is Success<string> formattersSuccess)
            {
                var jsonOptions = new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                    ReadCommentHandling = JsonCommentHandling.Skip
                };
                using JsonDocument formattersDoc = JsonDocument.Parse(formattersSuccess.Result, new JsonDocumentOptions()
                {
                    CommentHandling = JsonCommentHandling.Skip
                });
                var root = formattersDoc.RootElement;
                if (root.TryGetProperty("formatters", out var formattersEntry))
                {
                    foreach (JsonProperty jsonProperty in formattersEntry.EnumerateObject())
                    {
                        result[jsonProperty.Name] = jsonProperty.Value.GetRawText();
                    }
                }
            }
            return result;
        }
    }
}
