using Reqnroll.Bindings.Provider.Data;
using System.Text.Json.Serialization;

namespace Reqnroll.Bindings.Provider;

[JsonSourceGenerationOptions(
    PropertyNamingPolicy = JsonKnownNamingPolicy.Unspecified, // We specifiy the names explicitly
    UseStringEnumConverter = true)] // use strings instead of numbers for enums
[JsonSerializable(typeof(BindingData))]
internal partial class BindingJsonSourceGenerator : JsonSerializerContext
{

}
