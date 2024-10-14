using Io.Cucumber.Messages.Types;
using Reqnroll.CucumberMessages.PayloadProcessing.Cucumber;
using System;
using System.Text.Json;

namespace Reqnroll.CucumberMessages.PayloadProcessing
{
    /// <summary>
    /// When using System.Text.Json to serialize a Cucumber Message Envelope, the following serialization options are used.
    /// Consumers of Cucumber.Messages should use these options, or their serialization library's equivalent options.
    /// These options should work with System.Text.Json v6 or above.
    /// </summary>
    public class NdjsonSerializer
    {
        private static readonly Lazy<JsonSerializerOptions> _jsonOptions = new(() =>
        {
            var options = new JsonSerializerOptions();
            options.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
            options.Converters.Add(new CucumberMessageEnumConverter<AttachmentContentEncoding>());
            options.Converters.Add(new CucumberMessageEnumConverter<PickleStepType>());
            options.Converters.Add(new CucumberMessageEnumConverter<SourceMediaType>());
            options.Converters.Add(new CucumberMessageEnumConverter<StepDefinitionPatternType>());
            options.Converters.Add(new CucumberMessageEnumConverter<StepKeywordType>());
            options.Converters.Add(new CucumberMessageEnumConverter<TestStepResultStatus>());
            options.DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull;
            options.Encoder = System.Text.Encodings.Web.JavaScriptEncoder.Default;

            return options;
        });

        private static JsonSerializerOptions JsonOptions
        {
            get
            {
                return _jsonOptions.Value;
            }
        }

        public static string Serialize(Envelope message)
        {
            return Serialize<Envelope>(message);
        }

        internal static string Serialize<T>(T message)
        {
            return JsonSerializer.Serialize(message, JsonOptions);
        }

        public static Envelope Deserialize(string json)
        {
            return Deserialize<Envelope>(json);
        }

        internal static T Deserialize<T>(string json)
        {
            return JsonSerializer.Deserialize<T>(json, JsonOptions)!;
        }
    }
}
