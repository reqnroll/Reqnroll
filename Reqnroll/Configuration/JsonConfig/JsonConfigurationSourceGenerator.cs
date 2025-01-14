using System.Text.Json;
using System.Text.Json.Serialization;

namespace Reqnroll.Configuration.JsonConfig;

[JsonSourceGenerationOptions(WriteIndented = true,
    PropertyNamingPolicy = JsonKnownNamingPolicy.Unspecified, // We specifiy the names explicitly
    PropertyNameCaseInsensitive = true, // old custom parser supported ordinal ignore case, so we should do
    UseStringEnumConverter = true, // use strings instead of numbers for enums
    Converters = [typeof(CustomTimeSpanConverter)],
    ReadCommentHandling = JsonCommentHandling.Skip)] // the user can comment his used configuration value
[JsonSerializable(typeof(JsonConfig))]
internal partial class JsonConfigurationSourceGenerator : JsonSerializerContext
{

}
