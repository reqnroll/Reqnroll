using System;
using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Reqnroll.Configuration.JsonConfig
{
    /// <summary>
    /// Custom <see cref="TimeSpan"/> converter to stay compatible with old json format/parser
    /// </summary>
    sealed class CustomTimeSpanConverter : JsonConverter<TimeSpan>
    {
        public override TimeSpan Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            var content = reader.GetString();
            return TimeSpan.Parse(content, CultureInfo.InvariantCulture);
        }
        public override void Write(Utf8JsonWriter writer, TimeSpan value, JsonSerializerOptions options)
        {
            var content = value.ToString(null, CultureInfo.InvariantCulture);
            writer.WriteStringValue(content);
        }
    }
}
