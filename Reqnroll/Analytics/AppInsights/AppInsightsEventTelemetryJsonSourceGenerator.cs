using System.Text.Json.Serialization;

namespace Reqnroll.Analytics.AppInsights
{
    [JsonSourceGenerationOptions(
        PropertyNamingPolicy = JsonKnownNamingPolicy.Unspecified, // We specifiy the names explicitly
        UseStringEnumConverter = true)] // use strings instead of numbers for enums
    [JsonSerializable(typeof(AppInsightsEventTelemetry))]
    internal partial class AppInsightsEventTelemetryJsonSourceGenerator : JsonSerializerContext
    {

    }
}