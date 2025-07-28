using Io.Cucumber.Messages.Types;
using System;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;

namespace Reqnroll.Formatters.PayloadProcessing;

/// <summary>
/// Uses a correctly configured <see cref="JsonSerializer"/> that provides compatible JSON format for the Cucumber Messages standard.
/// These options should work with System.Text.Json v6 or above.
/// </summary>
public class NdjsonSerializer
{
    private static readonly Lazy<JsonSerializerOptions> _jsonOptions = new(() =>
    {
        var options = new JsonSerializerOptions();
        options.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
        options.Converters.Add(new CucumberMessagesEnumConverterFactory());
        options.DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull;
        options.Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping;

        return options;
    });

    private static JsonSerializerOptions JsonOptions => _jsonOptions.Value;

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

    public static async Task SerializeToStreamAsync(Stream fs, Envelope message)
    {
        await JsonSerializer.SerializeAsync(fs, message, JsonOptions);
    }
}