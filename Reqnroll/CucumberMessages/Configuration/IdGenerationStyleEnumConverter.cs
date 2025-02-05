using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Reqnroll.CucumberMessages.Configuration
{
    internal class IdGenerationStyleEnumConverter : JsonConverter<IDGenerationStyle> 
    {
        public override IDGenerationStyle Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            var value = reader.GetString();
            return ParseIdGenerationStyle(value);
        }

        public override void Write(Utf8JsonWriter writer, IDGenerationStyle value, JsonSerializerOptions options)
        {
            if (value == IDGenerationStyle.Incrementing)
                writer.WriteStringValue("INCREMENTING");
            else
                writer.WriteStringValue("UUID");
        }
        public static IDGenerationStyle ParseIdGenerationStyle(string idGenerationStyle)
        {
            if (string.IsNullOrEmpty(idGenerationStyle))
                idGenerationStyle = "UUID";

            if ("INCREMENTING".Equals(idGenerationStyle, StringComparison.OrdinalIgnoreCase))
                return IDGenerationStyle.Incrementing;
            else
                return IDGenerationStyle.UUID;
        }

    }
}
